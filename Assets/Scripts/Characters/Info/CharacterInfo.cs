using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            { EffectType.IncreaseMoveRange, ApplySimpleEffect },
            { EffectType.IncreaseActionPoints, ApplySimpleEffect },
            { EffectType.BloodSealEffect, ApplySimpleEffect },
            { EffectType.RedDahlia, ApplySimpleEffect },
            { EffectType.WhiteLotus, ApplySimpleEffect },
            { EffectType.Marigold, ApplySimpleEffect },
            { EffectType.NightCactus, ApplySimpleEffect },
            { EffectType.ReduceMoveRange, ApplySimpleEffect },
            { EffectType.ThietNhan_Infected, ApplySimpleEffect },
            { EffectType.Cover_50_Percent, ApplySimpleEffect },
            { EffectType.Cover_100_Percent, ApplySimpleEffect },
            { EffectType.DragonArmor, ApplySimpleEffect },
            { EffectType.SnakeArmor, ApplySimpleEffect },
            { EffectType.ReduceStat_PhamCuChich_Skill3, ApplySimpleEffect },
            { EffectType.Cover_PhamCuChich_Skill3, ApplySimpleEffect },
            { EffectType.BlockProjectile, ApplySimpleEffect },

            { EffectType.Sleep, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.Stun, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.Immobilize, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.ReduceChiDef, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.Disarm, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.Poison, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.ThietNhan_Poison, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.ThietNhan_ReduceMoveRange, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.ThietNhan_BlockAP, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.Taunt, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.Silence, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.ReduceAP, TryCheckEffectResistanceAndApplyEffect },
            { EffectType.Bleed, TryCheckEffectResistanceAndApplyEffect },
            
            { EffectType.IncreaseDamage, ApplyIncreaseDamage },
            { EffectType.BlockSkill, _ => ApplyBlockSkill() },
            { EffectType.BreakBloodSealDamage, ApplyBloodSealDamage },
            { EffectType.PoisonPowder, ApplyPoisonPowder },
            { EffectType.RemoveAllPoisonPowder, ApplyRemoveAllPoisonPowder },
            { EffectType.VenomousParasite, ApplyVenomousParasite },
            { EffectType.Shield, ApplyShieldEffect }
        };

        GameplayManager.Instance.OnEndTurn += OnEndTurn;
    }

    // Cell
    public int RoundIndex = 0;
    public Cell Cell { get; set; }
    public HashSet<Cell> MoveRange { get; set; } = new();

    public HashSet<Cell> SkillRange { get; set; } = new();

    // Skill
    public SkillInfo SkillInfo { get; set; }

    // Stat
    public bool IsToggleOn { get; set; } = false;
    public int Speed { get; set; }
    public CharacterAttributes Attributes { get; set; }
    public int CurrentHp { get; set; }
    public int CurrentMp { get; set; }
    public int MoveAmount { get; set; }
    public int DamageDealtInCurrentRound { get; set; } = 0;

    private int ShieldAmount => ShieldEffectData?.Value ?? 0;
    public bool IsDie => CurrentHp <= 0;

    private List<int> ActionPoints { get; set; } = new() { 3, 3, 3 };

    // Buff & Debuff
    private readonly Dictionary<EffectType, Action<EffectData>> _effectHandlers;
    public EffectInfo EffectInfo { get; } = new();

    public bool IsLockSkill => EffectInfo.Effects.Any(effect => effect.EffectType == EffectType.BlockSkill);
    private bool HasSleepEffect => EffectInfo.Effects.Any(p => p.EffectType == EffectType.Sleep);
    private bool HasStunEffect => EffectInfo.Effects.Any(p => p.EffectType == EffectType.Stun);
    private bool MustEndTurn => HasSleepEffect || HasStunEffect;

    public EffectData CoverEffectData =>
        EffectInfo.Effects.FirstOrDefault(p => p.EffectType == EffectType.Cover_50_Percent);

    public EffectData CoverFullDamageEffectData =>
        EffectInfo.Effects.FirstOrDefault(p => p.EffectType == EffectType.Cover_100_Percent);

    private EffectData DragonArmorEffectData =>
        EffectInfo.Effects.FirstOrDefault(p => p.EffectType == EffectType.DragonArmor);

    public ShieldEffect ShieldEffectData => (ShieldEffect)EffectInfo.Effects.FirstOrDefault(p => p.EffectType == EffectType.Shield);

    // Action
    public event EventHandler<int> OnHpChanged;
    public event EventHandler<int> OnMpChanged;

    public event EventHandler<float> OnShieldChanged;
    public event EventHandler<int> OnMoveAmount;
    public event EventHandler<int> OnNewRound;

    private SkillConfig SkillConfig { get; set; }
    private Character Character { get; set; }

    //
    private void HandleDamageTaken(DamageTakenParams damageTakenParams)
    {
        var damage = -damageTakenParams.Damage;
        if (damage == 0) return;
        TryCover_PhamCuChich_Skill3(ref damage);
        TryBreakDragonArmor(damage, damageTakenParams.ReceiveFromCharacter);
        TryHandleCoverEffect(ref damage);
        HandleHpChanged(ref damage, damageTakenParams.ReceiveFromCharacter);
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

    private void TryCover_PhamCuChich_Skill3(ref int damage)
    {
        if (EffectInfo.Effects.Any(p => p.EffectType == EffectType.Cover_PhamCuChich_Skill3))
        {
            Debug.Log($"[{Character.characterConfig.characterName}] có buff từ skill 3 của PCC => damage nhận = 1");
            damage = -1;
        }
    }

    private void TryBreakDragonArmor(int damage, Character character)
    {
        if (damage == 0) return;
        var dragonArmor = DragonArmorEffectData;
        if (dragonArmor == null) return;
        damage = -damage;
        var rollData = Roll.RollDice(2, 4, 0);
        if (damage > rollData)
        {
            dragonArmor.Actor.Info.RemoveAllEffect(EffectType.SnakeArmor);
            RemoveAllEffect(EffectType.DragonArmor);
            AlkawaDebug.Log(ELogCategory.EFFECT, $"Long Giáp: 2d4 = {rollData} < {damage} => vỡ khiên");
        }
        else
        {
            GameplayManager.Instance.MainCharacter.Info.HandleDamageTaken(damage, character);
            AlkawaDebug.Log(ELogCategory.EFFECT, $"Long Giáp: 2d4 = {rollData} < {damage} => phản sát thương");
        }
    }

    private void TryHandleCoverEffect(ref int damage)
    {
        // 100 %
        var fullCover = CoverFullDamageEffectData;
        if (fullCover != null)
        {
            fullCover.Actor.OnDamageTaken(new DamageTakenParams()
            {
                CanDodge = false,
                Damage = -damage,
                ReceiveFromCharacter = Character,
                CanCounter = false,
            });
            damage = 0;
            return;
        }

        // 50 %
        var coverEffect = CoverEffectData;
        if (coverEffect == null)
        {
            return;
        }

        damage = Utils.RoundNumber(damage * 1f / 2);
        coverEffect.Actor.OnDamageTaken(new DamageTakenParams()
        {
            CanDodge = false,
            Damage = -damage,
            ReceiveFromCharacter = Character,
            CanCounter = false,
        });
    }

    private void HandleDamageTaken(int damage, Character character)
    {
        if (damage == 0) return;
        HandleHpChanged(ref damage, character);
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

    private void HandleHpChanged(ref int hp, Character character)
    {
        if (ShieldEffectData != null && hp < 0)
        {
            var damage = -hp;
            var shieldValue = ShieldEffectData.Value;
            if (shieldValue >= damage)
            {
                ShieldEffectData.Value -= damage;
                HandleShieldChange(character);
                hp = 0;
            }
            else
            {
                var remainingDamage = damage - shieldValue;
                HandleShieldChange(character);
                ShieldEffectData.Value = 0;
                hp = -remainingDamage;
                HandleBreakShield(1, ShieldEffectData.Damage);
            }
        }

        CurrentHp += hp;
        character.Info.SetDamageDealtInCurrentRound(-hp);
        CurrentHp = Math.Max(0, CurrentHp);
    }

    private void SetDamageDealtInCurrentRound(int damage)
    {
        DamageDealtInCurrentRound += damage;
        Debug.Log($"[{Character.characterConfig.characterName}] Sát thương đã gây ra trong round = {DamageDealtInCurrentRound}");
    }
    
    private void HandleShieldChange(Character character)
    {
        int remainder = 0;

        if (ShieldAmount < 0)
        {
            remainder = -ShieldAmount;
        }
        
        OnShieldChanged?.Invoke(this, ShieldAmount * 1f / Attributes.health);

        if (remainder > 0)
        {
            HandleDamageTaken(-remainder, character);
        }
    }


    public void HandleMpChanged(int value)
    {
        if (value == 0) return;
        var dragon = DragonArmorEffectData;
        if (dragon != null)
        {
            if (dragon.Actor != null)
            {
                value = Utils.RoundNumber(value * 1f / 2f);
                dragon.Actor.Info.HandleMpChanged(value);
            }
            else
            {
                Debug.LogError("Loi roi");
            }
        }

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

    public int GetDef()
    {
        if (EffectInfo.Effects.Any(p => p.EffectType == EffectType.ReduceStat_PhamCuChich_Skill3))
        {
            return 0;
        }

        return Attributes.def;
    }

    private bool IsImmobilized()
    {
        return EffectInfo.Effects
            .Any(effect => effect.EffectType == EffectType.Immobilize);
    }

    private void OnEndTurn(object sender, EventArgs e)
    {
        RemoveAllEffect(EffectType.BlockSkill);
        DamageDealtInCurrentRound = 0;
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

        foreach (var effect in EffectInfo.Effects)
        {
            switch (effect.EffectType)
            {
                case EffectType.ReduceMoveRange:
                {
                    var reduction = Roll.RollDice(1, 4, 0);
                    AlkawaDebug.Log(
                        ELogCategory.EFFECT,
                        $"Apply debuff {effect.EffectType}: giảm di chuyển = 1d4 = {reduction}"
                    );
                    totalReduction += reduction;
                    break;
                }
                case EffectType.ThietNhan_ReduceMoveRange when effect is ChangeStatEffect changeStatEffect:
                    totalReduction += changeStatEffect.Value;
                    AlkawaDebug.Log(
                        ELogCategory.EFFECT,
                        $"Apply debuff {effect.EffectType}: giảm di chuyển = {changeStatEffect.Value}"
                    );
                    break;
            }
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
            switch (effect.EffectType)
            {
                case EffectType.Cover_PhamCuChich_Skill3:
                    effect.Actor.Info.RemoveEffect(EffectType.ReduceStat_PhamCuChich_Skill3, Character);
                    break;
                case EffectType.BlockProjectile:
                    if (effect is BlockProjectile projectile)
                    {
                        projectile.targetCell.UnSetMainProjectile();
                    }
                    break;
            }
            EffectInfo.Effects.Remove(effect);
        }

        Character.uiFeedback.UpdateEffectIcons();
    }

    public void ResetBuffBefore()
    {
        OnNewRound?.Invoke(this, RoundIndex);
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
        RoundIndex++;
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

    public List<int> ActionPointsList
    {
        get
        {
            List<int> combined = new List<int>();
        
            foreach (int ap in ActionPoints)
            {
                combined.Add(ap);
            }
        
            foreach (ActionPointEffect effect in GetActionPointEffects())
            {
                foreach (int ap in effect.ActionPoints)
                {
                    combined.Add(ap);
                }
            }
        
            combined.Reverse();

            int skipCount = 0;
            foreach (var effect in EffectInfo.Effects)
            {
                if (effect.EffectType == EffectType.ThietNhan_BlockAP)
                {
                    skipCount++;
                }
                else if (effect.EffectType == EffectType.ReduceAP && effect is ChangeStatEffect changeStatEffect)
                {
                    skipCount += changeStatEffect.Value;
                }
            }
        
            if (skipCount >= combined.Count)
            {
                return new List<int>();
            }
        
            List<int> temp = new List<int>();
            for (int i = skipCount; i < combined.Count; i++)
            {
                temp.Add(combined[i]);
            }
        
            temp.Reverse();
            return temp;
        }
    }
    
    private IEnumerable<ActionPointEffect> GetActionPointEffects()
    {
        foreach (var effect in EffectInfo.Effects)
        {
            if (effect is ActionPointEffect { EffectType: EffectType.IncreaseActionPoints } ape)
            {
                yield return ape;
            }
        }
    }


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
            for (var i = 0; i < points.Count; i++)
            {
                points[i] = Math.Min(points[i] + 1, 3);
            }
        }
    }

    private bool HasEnoughActionPoints => ActionPointsList.Any(point => point == 3);


    public void ReduceActionPoints()
    {
        var pointsToReduce = Character.GetSkillActionPoints(Character.GetSkillTurnType());

        if (TryReducePoints(GetActionPointEffects())) return;

        for (var i = 0; i < ActionPoints.Count; i++)
        {
            if (ActionPoints[i] != 3) continue;
            ActionPoints[i] -= pointsToReduce;
            RemoveAllEffect(EffectType.IncreaseActionPoints);
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
        ApplyEffects(damageTakenParams.Effects);
        HandleDamageTaken(damageTakenParams);
        HandleMpChanged(-damageTakenParams.ReducedMana);
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
                case EffectType.Poison:
                case EffectType.ThietNhan_Poison:
                {
                    if (item is PoisonEffectData poisonEffectData)
                    {
                        var rollData = poisonEffectData.Damage;
                        var damage = Roll.RollDice(rollData);
                        HandleDamageTaken(-damage, poisonEffectData.Actor);
                        Debug.Log(
                            $"[{Character.characterConfig.characterName}] - {item.EffectType}: Damage = {rollData.rollTime}d{rollData.rollValue} + {rollData.add} = {damage}");
                    }

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

    public void ApplyEffects(List<EffectData> effects)
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
        Character.uiFeedback.UpdateEffectIcons();
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

    private void ApplyEffect(EffectData effectData)
    {
        EffectInfo.AddEffect(effectData);
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {effectData.EffectType.ToString()}");
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
        ApplyEffect(effect);
    }

    private void ApplySimpleEffect(EffectData effectData)
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
        if (CurrentHp > 0) HandleDamageTaken(-damage, effectData.Actor);
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{Character.characterConfig.characterName}] Huyết Ấn: máu đã mất = {hpDecreased} => damage = {damage}");
    }

    private void TryCheckEffectResistanceAndApplyEffect(EffectData effectData)
    {
        if (ShouldApplyEffect(effectData))
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

    private void ApplyRemoveAllPoisonPowder(EffectData effectData)
    {
        RemoveAllEffect(effectData.EffectType);
    }

    public void RemoveAllEffect(EffectType effectType)
    {
        var value = EffectInfo.Effects.RemoveAll(p => p.EffectType == effectType);
        if (value <= 0) return;
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Removed effect: {effectType.ToString()}");
        GameplayManager.Instance.UpdateAllEffectFeedback();
    }

    public void RemoveEffect(EffectType effectType, Character actor)
    {
        for (int i = EffectInfo.Effects.Count - 1; i >= 0; i--)
        {
            var item = EffectInfo.Effects[i];
            if (item.EffectType != effectType || item.Actor != actor) continue;
            EffectInfo.Effects.RemoveAt(i);
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Removed effect: {effectType}");
            GameplayManager.Instance.UpdateAllEffectFeedback();
            return;
        }
    }

    
    public void HandleBreakShield(int range, int damage)
    {
        var validCharacters = GameplayManager.Instance.MapManager.GetCharactersInRange(Character.Info.Cell,
            new SkillInfo()
            {
                range = range,
                damageType = DamageTargetType.Enemies,
            });
        foreach (var character in validCharacters)
        {
            character.Info.OnDamageTaken(new DamageTakenParams()
            {
                Damage = damage,
            });
        }
        Debug.Log($"Vỡ shield gây {damage} damage trong phạm vi {range} ô");
        RemoveAllEffect(EffectType.Shield);
        HandleShieldChange(Character);
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

    private void ApplyShieldEffect(EffectData effectData)
    {
        if (effectData is not ShieldEffect changeStatEffect) return;
        if (ShieldEffectData == null)
        {
            ApplySimpleEffect(effectData);
        }
        else
        {
            ShieldEffectData.Duration = changeStatEffect.Duration;
            ShieldEffectData.Value += changeStatEffect.Value;
            ShieldEffectData.Damage = Mathf.Max(ShieldEffectData.Damage, changeStatEffect.Damage);
        }

        if (ShieldEffectData != null)
        {
            Debug.Log($"Nhận lượng shield = {changeStatEffect} | lương shield hiện tại = {ShieldEffectData.Value}");
            Debug.Log($"damage khi nổ shield = {ShieldEffectData.Damage}");
            HandleShieldChange(Character);
        }
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