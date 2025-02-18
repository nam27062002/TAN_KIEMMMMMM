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

        _effectHandlers = new Dictionary<EffectType, Action<EffectData>>
        {
            { EffectType.IncreaseDamage, ApplyIncreaseDamage },
            { EffectType.BlockSkill, _ => ApplyBlockSkill() },
            { EffectType.IncreaseMoveRange, ApplyIncreaseMoveRange },
            { EffectType.IncreaseActionPoints, ApplyIncreaseActionPoints },
            { EffectType.BloodSealEffect, ApplyBloodSealEffect },
            { EffectType.BreakBloodSealDamage, ApplyBloodSealDamage },
            { EffectType.Immobilize, ApplyImmobilize },
            { EffectType.ReduceMoveRange, ApplyReduceMoveRange },
            { EffectType.PoisonPowder, ApplyPoisonPowder },
            { EffectType.Sleep, ApplySleep },
            { EffectType.Stun, ApplyStun },
            { EffectType.RedDahlia, ApplyRedDahlia },
            { EffectType.WhiteLotus, ApplyWhiteLotus },
            { EffectType.Marigold, ApplyMarigold },
            { EffectType.NightCactus, ApplyNightCactus },
            { EffectType.RemoveAllPoisonPowder, ApplyRemoveAllPoisonPowder },
            { EffectType.ReduceChiDef, ApplyReduceChiDef },
            { EffectType.VenomousParasite, ApplyVenomousParasite },
            { EffectType.Poison, ApplyPoison }
        };
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
    private readonly Dictionary<EffectType, Action<EffectData>> _effectHandlers;
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
        var buff = EffectInfo.Effects
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
        ApplyEffects(damageTakenParams.Effects);
        Character.uiFeedback.UpdateEffectIcons();
    }

    public void CheckEffectAfterReceiveDamage(DamageTakenParams damageTakenParams)
    {
        // remove all sleep
        if (damageTakenParams.Effects.All(p => p.EffectType != EffectType.Sleep))
        {
            if (EffectInfo.Effects.Any(p => p.EffectType == EffectType.Sleep))
            {
                EffectInfo.Effects.RemoveAll(p => p.EffectType == EffectType.Sleep);
                AlkawaDebug.Log(ELogCategory.EFFECT,
                    $"[{Character.characterConfig.characterName}] nhận damage => removed effect: Sleep");
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
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"{item.EffectType}: Giải hiệu ứng = {effectCleanse} - Quy ước: {baseEffectCleanse}");
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
            Debug.Log(
                $"[{Character.characterConfig.characterName}] sleep = {HasSleepEffect} or stun = {HasStunEffect} => End turn");
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

    private void ApplyEffects(List<EffectData> effects)
    {
        foreach (var effect in effects)
        {
            if (_effectHandlers.TryGetValue(effect.EffectType, out var handler))
            {
                handler(effect);
            }
            else
            {
                AlkawaDebug.Log(ELogCategory.EFFECT,
                    $"[{Character.characterConfig.characterName}] No handler for effect: {effect.EffectType}");
            }
        }
    }
    
    private bool ShouldApplyEffect(EffectData effectData)
    {
        var effectResistance = _roll.GetEffectResistance();
        var baseEffectResistance = EffectInfo.AppliedEffect[effectData.EffectType].Item1;
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"Debuff {effectData.EffectType}: Kháng hiệu ứng = {effectResistance} - Quy ước: {baseEffectResistance}");
#if !ALWAY_APPLY_EFFECT
    return effectResistance < baseEffectResistance;
#else
        return true;
#endif
    }

    private void ApplyEffect(EffectData effectData, string effectName = null)
    {
        EffectInfo.AddEffect(effectData);
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {effectName ?? effectData.EffectType.ToString()}");
    }
    
    private void ApplyIncreaseDamage(EffectData effectData)
    {
        if (effectData is not ChangeStatEffect changeStatEffect || changeStatEffect.Value == 0)
            return;
        ApplyEffect(effectData);
        Character.ShowMessage($"Tăng {changeStatEffect.Value} sát thương");
    }

    private void ApplyBlockSkill()
    {
        var effect = new EffectData { Duration = 1, EffectType = EffectType.BlockSkill };
        ApplyEffect(effect, EffectType.BlockSkill.ToString());
    }

    private void ApplyIncreaseMoveRange(EffectData effectData)
    {
        ApplyEffect(effectData);
    }

    private void ApplyIncreaseActionPoints(EffectData effectData)
    {
        ApplyEffect(effectData);
    }

    private void ApplyBloodSealEffect(EffectData effectData)
    {
        ApplyEffect(effectData);
    }

    private void ApplyBloodSealDamage(EffectData effectData)
    {
        if (EffectInfo.Effects.All(p => p.EffectType != EffectType.BloodSealEffect))
            return;
        EffectInfo.Effects.RemoveAll(p => p.EffectType == EffectType.BloodSealEffect);
        var hpDecreased = Attributes.health - CurrentHp;
        var damage = Utils.RoundNumber(hpDecreased * 1f / 10);
        if (CurrentHp > 0) HandleHpChanged(-damage);
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{Character.characterConfig.characterName}] Huyết Ấn: máu đã mất = {hpDecreased} => damage = {damage}");
    }

    private void ApplyImmobilize(EffectData effectData)
    {
        if (ShouldApplyEffect(effectData))
            ApplyEffect(effectData);
    }

    private void ApplyReduceMoveRange(EffectData effectData)
    {
        ApplyEffect(effectData);
    }

    private void ApplyPoisonPowder(EffectData effectData)
    {
        var rollData = Roll.RollDice(1, 20, 0);
#if !ALWAY_APPLY_EFFECT
    if (rollData < 10)
#endif
        {
            EffectInfo.AddEffect(effectData);
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] roll data = {rollData} < 10 => Added effect: Độc Phấn");
        }
#if !ALWAY_APPLY_EFFECT
    else
    {
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] roll data = {rollData} >= 10 => can't add effect: Độc Phấn");
    }
#endif
    }

    private void ApplyRedDahlia(EffectData effectData)
    {
        ApplyEffect(effectData, "Hoa Thược Dược");
    }

    private void ApplyWhiteLotus(EffectData effectData)
    {
        ApplyEffect(effectData, "Hoa Sen Trắng");
    }

    private void ApplyMarigold(EffectData effectData)
    {
        ApplyEffect(effectData, "Hoa Cúc Vạn Thọ");
    }

    private void ApplyNightCactus(EffectData effectData)
    {
        ApplyEffect(effectData, "Hoa Quỳnh");
    }

    private void ApplySleep(EffectData effectData)
    {
        if (ShouldApplyEffect(effectData))
            ApplyEffect(effectData);
    }

    private void ApplyStun(EffectData effectData)
    {
        if (ShouldApplyEffect(effectData))
            ApplyEffect(effectData);
    }

    private void ApplyRemoveAllPoisonPowder(EffectData effectData)
    {
        EffectInfo.Effects.RemoveAll(p => p.EffectType == EffectType.PoisonPowder);
    }

    private void ApplyReduceChiDef(EffectData effectData)
    {
        if (ShouldApplyEffect(effectData))
            ApplyEffect(effectData);
    }

    private void ApplyVenomousParasite(EffectData effectData)
    {
        if (effectData is ChangeStatEffect changeStatEffect)
        {
            var removedCount = 0;
            foreach (var effect in EffectInfo.Effects.ToList().Where(e => IsVenomousEffect(e.EffectType)))
            {
                EffectInfo.Effects.Remove(effect);
                AlkawaDebug.Log(ELogCategory.EFFECT,
                    $"[{Character.characterConfig.characterName}] Removed effect: {effect.EffectType}");
                removedCount++;
                if (removedCount >= changeStatEffect.Value)
                    break;
            }
        }
    }

    private void ApplyPoison(EffectData effectData)
    {
        if (ShouldApplyEffect(effectData))
            ApplyEffect(effectData);
    }
    
    public int GetPoisonPowder()
    {
        return EffectInfo.Effects.Count(p => p.EffectType == EffectType.PoisonPowder);
    }

    public int CountFlower()
    {
        return EffectInfo.Effects.Count(effect =>
            effect.EffectType is EffectType.RedDahlia or EffectType.WhiteLotus or EffectType.Marigold
                or EffectType.NightCactus);
    }

    private bool IsVenomousEffect(EffectType effectType) =>
        effectType is EffectType.RedDahlia or EffectType.Marigold or EffectType.NightCactus or EffectType.WhiteLotus;

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