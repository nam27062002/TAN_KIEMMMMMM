using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR;

public class CharacterInfo
{
    public CharacterInfo(SkillConfig skillConfig, CharacterAttributes characterAttributes, Character character)
    {
        SkillConfig = skillConfig;
        Attributes = characterAttributes;
        Character = character;

        CurrentHp = characterAttributes.health;
        CurrentMp = characterAttributes.mana;
        _roll = new Roll(this, Character.characterConfig.characterName);
    }

    // Cell
    public Cell Cell { get; set; }
    public HashSet<Cell> MoveRange { get; set; } = new();

    public HashSet<Cell> SkillRange { get; set; } = new();

    // Skill
    public SkillInfo SkillInfo { get; set; }

    // Character
    public HashSet<Character> CharactersInSkillRange { get; set; } = new();

    // Stat
    public bool IsToggleOn { get; set; } = false;
    public int Speed { get; set; }
    public CharacterAttributes Attributes { get; set; }
    public int CurrentHp { get; set; }
    public int CurrentMp { get; set; }
    public int MoveAmount { get; set; }

    public bool IsDie => CurrentHp <= 0;

    private List<int> ActionPoints { get; set; } = new() { 3, 3, 3 };

    // Buff & Debuff

    public EffectInfo EffectInfo { get; } = new();

    public bool IsLockSkill => EffectInfo.Effects.Any(effect => effect.EffectType == EffectType.BlockSkill);
    private bool HasSleepEffect => EffectInfo.Effects.Any(p => p.EffectType == EffectType.Sleep);
    private bool HasStunEffect => EffectInfo.Effects.Any(p => p.EffectType == EffectType.Stun);
    
    private bool MustEndTurn => HasSleepEffect || HasStunEffect;

    // Action
    public event EventHandler<int> OnHpChanged;
    public event EventHandler<int> OnMpChanged;
    public event EventHandler<int> OnMoveAmount;

    public SkillConfig SkillConfig { get; set; }
    public Character Character { get; set; }

    //
    private void HandleHpChanged(DamageTakenParams damageTakenParams)
    {
        var damage = -damageTakenParams.Damage;
        if (damage == 0) return;
        CurrentHp += damage;
        CurrentHp = math.max(0, CurrentHp);
        if (IsDie)
        {
            damageTakenParams.OnSetDamageTakenFinished?.Invoke(new FinishApplySkillParams()
            {
                Character = Character,
                WaitForCounter = false,
            });
            Character.OnDie();
        }
        else
        {
            Character.ShowMessage($"{-damage}");
            OnHpChanged?.Invoke(this, damageTakenParams.Damage);
        }
    }

    private void HandleHpChanged(int damage)
    {
        if (damage == 0) return;
        CurrentHp += damage;
        CurrentHp = math.max(0, CurrentHp);
        if (IsDie)
        {
            Character.OnDie();
        }
        else
        {
            Character.ShowMessage($"{-damage}");
            OnHpChanged?.Invoke(this, damage);
        }
    }

    public void HandleMpChanged(int value)
    {
        if (value == 0) return;
        CurrentMp += value;
        OnMpChanged?.Invoke(this, value);
    }

    public void HandleMoveAmountChanged(int value)
    {
        OnMoveAmount?.Invoke(this, value);
    }

    public int GetMoveRange()
    {
        if (IsImmobilized())
            return 0;

        var baseMovement = CalculateBaseMovement();
        var totalReduction = CalculateMoveReduction();
    
        return Mathf.Max(0, baseMovement - totalReduction);
    }

    private bool IsImmobilized()
    {
        return EffectInfo.Effects
            .Any(effect => effect.EffectType == EffectType.Immobilize);
    }

    private int CalculateBaseMovement()
    {
        var baseValue = Attributes.maxMoveRange - MoveAmount;
        var moveBuff = EffectInfo.Effects
            .Where(e => e.EffectType == EffectType.IncreaseMoveRange)
            .Sum(e => ((ChangeStatEffect)e).Value);
        return baseValue + moveBuff;
    }

    public int CalculateChiDef()
    {
        var chiDef = Attributes.chiDef;
        var buff =  EffectInfo.Effects
            .Where(e => e.EffectType == EffectType.ReduceChiDef)
            .Sum(e => ((ChangeStatEffect)e).Value);
        return chiDef - buff;
    }

    private int CalculateMoveReduction()
    {
        var totalReduction = 0;
    
        foreach (var unused in EffectInfo.Effects
                     .Where(e => e.EffectType == EffectType.ReduceMoveRange))
        {
            var reduction = Roll.RollDice(1, 4, 0);
            AlkawaDebug.Log(
                ELogCategory.EFFECT,
                $"Apply debuff 16: giảm di chuyển = 1d4 = {reduction}"
            );
            totalReduction += reduction;
        }
    
        return totalReduction;
    }

    public void ResetBuffAfter()
    {
        foreach (var effect in EffectInfo.Effects.ToList()
                     .Where(effect => EffectInfo.TriggerAtEnd.Contains(effect.EffectType)))
        {
            effect.Duration--;
            if (effect.Duration != 0) continue;
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Removed effect: {effect.EffectType}");
            EffectInfo.Effects.Remove(effect);
        }
        
        Character.uiFeedback.UpdateEffectIcons();
    }

    public void ResetBuffBefore()
    {
        MoveAmount = 0;
        IncreaseActionPointsValue();
        foreach (var effect in EffectInfo.Effects.ToList()
                     .Where(effect => EffectInfo.TriggerAtStart.Contains(effect.EffectType)))
        {
            effect.Duration--;
            if (effect.Duration != 0) continue;
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Removed effect: {effect.EffectType}");
            EffectInfo.Effects.Remove(effect);
        }
        HandleEffectCleanse();
        HandleApplyEffectBeforeNewRound();
        Character.uiFeedback.UpdateEffectIcons();
    }

    public SkillInfo GetSkillInfo(int index, SkillTurnType skillTurnType)
    {
        return SkillConfig.SkillConfigs[skillTurnType][index];
    }

    public bool CanCastSkill(SkillInfo skillInfo)
    {
        return CurrentMp >= skillInfo.mpCost && HasEnoughActionPoints;
    }

    #region Action Points

    public List<int> ActionPointsList => new List<int>(ActionPoints)
        .Concat(GetActionPointEffects()
            .SelectMany(e => e.ActionPoints))
        .ToList();

    private IEnumerable<ActionPointEffect> GetActionPointEffects() =>
        EffectInfo.Effects
            .OfType<ActionPointEffect>()
            .Where(e => e.EffectType == EffectType.IncreaseActionPoints);

    private void IncreaseActionPointsValue()
    {
        IncreasePoints(ActionPoints);

        foreach (var effect in GetActionPointEffects())
        {
            IncreasePoints(effect.ActionPoints);
        }

        return;

        void IncreasePoints(IList<int> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = Math.Min(points[i] + 1, 3);
            }
        }
    }

    private bool HasEnoughActionPoints => ActionPointsList.Any(point => point == 3);

    private int GetSkillActionPoints(SkillTurnType skillTurnType) =>
        Character.characterConfig.actionPoints[skillTurnType];

    public void ReduceActionPoints()
    {
        var pointsToReduce = GetSkillActionPoints(Character.GetSkillTurnType());

        if (TryReducePoints(GetActionPointEffects())) return;

        for (var i = 0; i < ActionPoints.Count; i++)
        {
            if (ActionPoints[i] != 3) continue;
            ActionPoints[i] -= pointsToReduce;
            return;
        }

        return;

        bool TryReducePoints(IEnumerable<ActionPointEffect> effects)
        {
            foreach (var effect in effects)
            {
                for (var i = 0; i < effect.ActionPoints.Count; i++)
                {
                    if (effect.ActionPoints[i] != 3) continue;
                    effect.ActionPoints[i] -= pointsToReduce;
                    return true;
                }
            }

            return false;
        }
    }

    #endregion

    #region Damage Logic

    public int GetCurrentDamage()
    {
        var damage = Attributes.atk;
        foreach (var effectData in EffectInfo.Effects)
        {
            if (effectData.EffectType == EffectType.IncreaseDamage && effectData is ChangeStatEffect changeStatEffect)
            {
                damage += changeStatEffect.Value;
            }
        }

        return damage;
    }

    public void OnDamageTaken(DamageTakenParams damageTakenParams)
    {
        HandleHpChanged(damageTakenParams);
        HandleMpChanged(-damageTakenParams.ReducedMana);
        ApplyEffect(damageTakenParams.Effects);
        Character.uiFeedback.UpdateEffectIcons();
    }

    public void CheckEffectAfterReceiveDamage(DamageTakenParams damageTakenParams)
    {
        // remove all sleep
        if (!damageTakenParams.Effects.ContainsKey(EffectType.Sleep))
        {
            if (EffectInfo.Effects.Any(p => p.EffectType == EffectType.Sleep))
            {
                EffectInfo.Effects.RemoveAll(p => p.EffectType == EffectType.Sleep);
                AlkawaDebug.Log(ELogCategory.EFFECT, $"[{Character.characterConfig.characterName}] nhận damage => removed effect: Sleep");
            }
        }
        
        Character.uiFeedback.UpdateEffectIcons();
    }
    
    private void HandleEffectCleanse()
    {
        foreach (var item in EffectInfo.Effects.ToList())
        {
            if (!EffectInfo.AppliedEffect.TryGetValue(item.EffectType, out var value)) continue;
            var effectCleanse = _roll.GetEffectCleanse();
            var baseEffectCleanse = value.Item2;
            AlkawaDebug.Log(ELogCategory.EFFECT, $"{item.EffectType}: Giải hiệu ứng = {effectCleanse} - Quy ước: {baseEffectCleanse}");
#if !ALWAY_APPLY_EFFECT
            if (effectCleanse >= baseEffectCleanse) continue;
                    EffectInfo.Effects.Remove(item);
#endif
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Removed effect: {item.EffectType}");
        }    
    }

    private void HandleApplyEffectBeforeNewRound()
    {
        foreach (var item in EffectInfo.Effects.ToList())
        {
            switch (item.EffectType)
            {
                case EffectType.Immobilize:
                {
                    var damage = Roll.RollDice(1, 6, 0);
                    HandleHpChanged(-damage);
                    Debug.Log($"[{Character.characterConfig.characterName}] - Mổng Yểm: Damage = 1d6 = {damage}");
                    break;
                }
                case EffectType.Poison:
                {
                    var damage = Roll.RollDice(1, 4, 0);
                    HandleHpChanged(-damage);
                    Debug.Log($"[{Character.characterConfig.characterName}] - Poison: Damage = 1d4 = {damage}");
                    break;
                }
            }
        }

        if (MustEndTurn)
        {
            Debug.Log($"[{Character.characterConfig.characterName}] sleep = {HasSleepEffect} or stun = {HasStunEffect} => End turn");
            GameplayManager.Instance.HandleEndTurn();
        }
        else
        {
            if (Character.Type == Type.AI)
            {
                ((AICharacter)Character).HandleAIPlayCoroutine();
            }
        }
    }
    
    private void ApplyEffect(Dictionary<EffectType, int> effects)
    {
        if (effects.TryGetValue(EffectType.IncreaseDamage, out var damage))
        {
            ApplyIncreaseDamage(damage);
        }

        if (effects.TryGetValue(EffectType.BlockSkill, out var _))
        {
            ApplyBlockSkill();
        }

        if (effects.TryGetValue(EffectType.IncreaseMoveRange, out var moveRange))
        {
            ApplyIncreaseMoveRange(moveRange);
        }

        if (effects.TryGetValue(EffectType.IncreaseActionPoints, out var actionPoints))
        {
            ApplyIncreaseActionPoints(actionPoints);
        }

        if (effects.TryGetValue(EffectType.BloodSealEffect, out var _))
        {
            ApplyBloodSealEffect();
        }

        if (effects.TryGetValue(EffectType.BreakBloodSealDamage, out var _))
        {
            ApplyBloodSealDamage();
        }

        if (effects.TryGetValue(EffectType.Immobilize, out var duration))
        {
            ApplyImmobilize(duration);
        }

        if (effects.TryGetValue(EffectType.ReduceMoveRange, out duration))
        {
            ApplyReduceMoveRange(duration);
        }

        if (effects.TryGetValue(EffectType.PoisonPowder, out var _))
        {
            ApplyPoisonPowder();
        }

        if (effects.TryGetValue(EffectType.Sleep, out var _))
        {
            ApplySleep();
        }
        
        if (effects.TryGetValue(EffectType.Stun, out var _))
        {
            ApplyStun();
        }

        if (effects.TryGetValue(EffectType.RedDahlia, out var _))
        {
            ApplyRedDahlia();
        }
        
        if (effects.TryGetValue(EffectType.WhiteLotus, out var _))
        {
            ApplyWhiteLotus();
        }
        
        if (effects.TryGetValue(EffectType.Marigold, out var _))
        {
            ApplyMarigold();
        }

        if (effects.TryGetValue(EffectType.NightCactus, out var _))
        {
            ApplyNightCactus();
        }

        if (effects.TryGetValue(EffectType.RemoveAllPoisonPowder, out var _))
        {
            ApplyRemoveAllPoisonPowder();
        }
        if (effects.TryGetValue(EffectType.ReduceChiDef, out var value))
        {
            ApplyReduceChiDef(value);
        }

        if (effects.TryGetValue(EffectType.VenomousParasite, out value))
        {
            ApplyVenomousParasite(value);
        }

        if (effects.TryGetValue(EffectType.Poison, out duration))
        {
            ApplyPoison(duration);
        }
    }

    private void ApplyReduceChiDef(int value)
    {
        var effectResistance = _roll.GetEffectResistance();
        var baseEffectResistance = EffectInfo.AppliedEffect[EffectType.ReduceChiDef].Item1;
        AlkawaDebug.Log(ELogCategory.EFFECT, $"Debuff 7: Kháng hiệu ứng = {effectResistance} - Quy ước: {baseEffectResistance}");
#if !ALWAY_APPLY_EFFECT
        if (effectResistance < baseEffectResistance)
#endif
        {
            EffectInfo.AddEffect(new ChangeStatEffect()
            {
                Value =  value,
                EffectType = EffectType.ReduceChiDef,
                Duration = EffectConfig.DebuffRound,
            });
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Added effect: {EffectType.ReduceChiDef}");
        }
    }

    private bool IsVenomousEffect(EffectType effectType) =>
        effectType is EffectType.RedDahlia or EffectType.Marigold or EffectType.NightCactus or EffectType.WhiteLotus;

    private void ApplyVenomousParasite(int removalCount)
    {
        var removedCount = 0;
        foreach (var effect in EffectInfo.Effects.ToList().Where(effect => IsVenomousEffect(effect.EffectType)))
        {
            EffectInfo.Effects.Remove(effect);
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Removed effect: {effect.EffectType}");
            removedCount++;
            if (removedCount >= removalCount)
                break;
        }
    }
    
    private void ApplyIncreaseDamage(int damage)
    {
        if (damage == 0) return;
        EffectInfo.AddEffect(new ChangeStatEffect
        {
            Value = damage,
            Duration = EffectConfig.BuffRound,
            EffectType = EffectType.IncreaseDamage,
        });
        Character.ShowMessage($"Tăng {damage} sát thương");
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.IncreaseDamage}");
    }

    private void ApplyBlockSkill()
    {
        EffectInfo.AddEffect(new EffectData()
        {
            Duration = 1,
            EffectType = EffectType.BlockSkill,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.BlockSkill}");
    }

    private void ApplyIncreaseMoveRange(int moveRange)
    {
        EffectInfo.AddEffect(new ChangeStatEffect()
        {
            Value = moveRange,
            Duration = 1,
            EffectType = EffectType.IncreaseMoveRange,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.IncreaseMoveRange}");
    }

    private void ApplyIncreaseActionPoints(int actionPoints)
    {
        var actionPoint = new List<int>();
        for (var i = 0; i < actionPoints; i++)
        {
            actionPoint.Add(3);
        }

        EffectInfo.AddEffect(new ActionPointEffect()
        {
            ActionPoints = actionPoint,
            Duration = 1,
            EffectType = EffectType.IncreaseActionPoints,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.IncreaseActionPoints}");
    }

    private void ApplyBloodSealEffect()
    {
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.BloodSealEffect,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.BloodSealEffect}");
    }

    private void ApplyBloodSealDamage()
    {
        if (EffectInfo.Effects.Any(p => p.EffectType == EffectType.BloodSealEffect))
        {
            EffectInfo.Effects.RemoveAll(p => p.EffectType == EffectType.BloodSealEffect);
            var hpDecreased = Attributes.health - CurrentHp;
            var damage = hpDecreased / 10;
            if (CurrentHp > 0) HandleHpChanged(-damage);
            AlkawaDebug.Log(ELogCategory.SKILL,
                $"[{Character.characterConfig.characterName}] Huyết Ấn: máu đã mất = {hpDecreased} => damage = {damage}");
        }
    }

    private void ApplyImmobilize(int duration)
    {
        var effectResistance = _roll.GetEffectResistance();
        var baseEffectResistance = EffectInfo.AppliedEffect[EffectType.Immobilize].Item1;
        AlkawaDebug.Log(ELogCategory.EFFECT, $"Debuff 15: Kháng hiệu ứng = {effectResistance} - Quy ước: {baseEffectResistance}");
#if !ALWAY_APPLY_EFFECT
        if (effectResistance < baseEffectResistance)
#endif
        {
            EffectInfo.AddEffect(new EffectData()
            {
                EffectType = EffectType.Immobilize,
                Duration = duration
            });
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Added effect: {EffectType.Immobilize}");
        }
    }

    private void ApplyReduceMoveRange(int duration)
    {
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.ReduceMoveRange,
            Duration = duration
        });
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.ReduceMoveRange}");
    }

    private void ApplyPoisonPowder()
    {
        var rollData = Roll.RollDice(1, 20, 0);
#if !ALWAY_APPLY_EFFECT
        if (rollData < 10)
#endif
        {
            EffectInfo.AddEffect(new EffectData()
            {
                EffectType = EffectType.PoisonPowder
            });
        }
        AlkawaDebug.Log(ELogCategory.EFFECT,
            rollData < 10
                ? $"[{Character.characterConfig.characterName}] roll data = {rollData} < 10 => Added effect: Độc Phấn"
                : $"[{Character.characterConfig.characterName}] roll data = {rollData} >= 10 => can't add effect: Độc Phấn");
    }

    private void ApplyRedDahlia()
    {
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.RedDahlia,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT, $"[{Character.characterConfig.characterName}] Added effect: Hoa Thược Dược");
    }

    private void ApplyWhiteLotus()
    {
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.WhiteLotus,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT, $"[{Character.characterConfig.characterName}] Added effect: Hoa Sen Trắng");
    }

    private void ApplyMarigold()
    {
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.Marigold,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT, $"[{Character.characterConfig.characterName}] Added effect: Hoa Cúc Vạn Thọ");
    }

    private void ApplyNightCactus()
    {
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.NightCactus,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT, $"[{Character.characterConfig.characterName}] Added effect: Hoa Quỳnh");   
    }

    private void ApplySleep()
    {
        var effectResistance = _roll.GetEffectResistance();
        var baseEffectResistance = EffectInfo.AppliedEffect[EffectType.Sleep].Item1;
        AlkawaDebug.Log(ELogCategory.EFFECT, $"Debuff 4: Kháng hiệu ứng = {effectResistance} - Quy ước: {baseEffectResistance}");
#if !ALWAY_APPLY_EFFECT
        if (effectResistance >= baseEffectResistance) return;
#endif
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.Sleep,
            Duration = EffectConfig.DebuffRound,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.Sleep}");
    }
    
    private void ApplyStun()
    {
        var effectResistance = _roll.GetEffectResistance();
        var baseEffectResistance = EffectInfo.AppliedEffect[EffectType.Stun].Item1;
        AlkawaDebug.Log(ELogCategory.EFFECT, $"Debuff 14: Kháng hiệu ứng = {effectResistance} - Quy ước: {baseEffectResistance}");
#if !ALWAY_APPLY_EFFECT
        if (effectResistance >= baseEffectResistance) return;
#endif
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.Stun,
            Duration = EffectConfig.DebuffRound,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.Stun}");
    }

    private void ApplyRemoveAllPoisonPowder()
    {
        EffectInfo.Effects.RemoveAll(p => p.EffectType == EffectType.PoisonPowder);
    }

    private void ApplyPoison(int duration)
    {
        var effectResistance = _roll.GetEffectResistance();
        var baseEffectResistance = EffectInfo.AppliedEffect[EffectType.Poison].Item1;
        AlkawaDebug.Log(ELogCategory.EFFECT, $"Debuff 12: Kháng hiệu ứng = {effectResistance} - Quy ước: {baseEffectResistance}");
#if !ALWAY_APPLY_EFFECT
        if (effectResistance >= baseEffectResistance) return;
#endif
        EffectInfo.AddEffect(new EffectData()
        {
            EffectType = EffectType.Poison,
            Duration = duration,
        });
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {EffectType.Poison}");
    }

    public int GetPoisonPowder()
    {
        return EffectInfo.Effects.Count(p => p.EffectType == EffectType.PoisonPowder);
    }

    public int CountFlower()
    {
        return EffectInfo.Effects.Count(effect => effect.EffectType is EffectType.RedDahlia or EffectType.WhiteLotus or EffectType.Marigold or EffectType.NightCactus);
    }
    #endregion

    #region Roll

    private readonly Roll _roll;
    public int BaseDamage => _roll.GetBaseDamage();
    public HitChangeParams HitChangeParams => _roll.GetHitChange();
    public int Dodge => _roll.GetDodge();

    public void SetSpeed()
    {
        Speed = _roll.GetSpeed();
    }

    #endregion
}