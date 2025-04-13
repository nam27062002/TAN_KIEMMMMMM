using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
[Serializable]
public class CharacterInfo
{
    public CharacterInfo(SkillConfig skillConfig, CharacterAttributes characterAttributes, Character character)
    {
        SkillConfig = skillConfig;
        Attributes = characterAttributes;
        Character = character;
#if UNITY_EDITOR
        CurrentHp = Attributes.overrideMaxHp ? characterAttributes.maxHpOverride : characterAttributes.health;
#else
        CurrentHp = characterAttributes.health;
#endif
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
            { EffectType.IncreaseDef, ApplySimpleEffect },
            { EffectType.IncreaseSpd, ApplySimpleEffect },
            { EffectType.ReduceHitChange, ApplySimpleEffect },
            { EffectType.LifeSteal, ApplySimpleEffect },
            { EffectType.Prone, ApplySimpleEffect },
            { EffectType.Fear, ApplySimpleEffect },
            { EffectType.Drunk, ApplySimpleEffect },
            { EffectType.SetDefToZero, ApplySimpleEffect },

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
            { EffectType.Blind, TryCheckEffectResistanceAndApplyEffect },

            { EffectType.IncreaseDamage, ApplyIncreaseDamage },
            { EffectType.BlockSkill, _ => ApplyBlockSkill() },
            { EffectType.BreakBloodSealDamage, ApplyBloodSealDamage },
            { EffectType.PoisonPowder, ApplyPoisonPowder },
            { EffectType.RemoveAllPoisonPowder, ApplyRemoveAllPoisonPowder },
            { EffectType.VenomousParasite, ApplyVenomousParasite },
            { EffectType.Shield, ApplyShieldEffect },
            { EffectType.PoisonousBloodPool, ApplyPoisonousBloodPool},
        };

        GameplayManager.Instance.OnEndTurn += OnEndTurn;
    }

    // Cell
    public int RoundIndex = 0;
    public Cell Cell { get; set; }
    public HashSet<Cell> MoveRange { get; set; } = new();
    public HashSet<Cell> SkillRange { get; set; } = new();
    public bool IsFirstRoundAfterLoad { get; set; } = false; // Biến cờ đánh dấu lần đầu sau khi load

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

    private int ShieldAmount => ShieldEffectData?.value ?? 0;
    public bool IsDie => CurrentHp <= 0;

    public List<int> ActionPoints { get; set; } = new() { 3, 3, 3 };

    // Buff & Debuff
    private readonly Dictionary<EffectType, Action<EffectData>> _effectHandlers;
    [ShowInInspector] public EffectInfo EffectInfo { get; } = new();

    public bool IsLockSkill => EffectInfo.Effects.Any(effect => effect.effectType == EffectType.BlockSkill);
    private bool HasSleepEffect => EffectInfo.Effects.Any(p => p.effectType == EffectType.Sleep);
    private bool HasStunEffect => EffectInfo.Effects.Any(p => p.effectType == EffectType.Stun);
    public bool MustEndTurn => HasSleepEffect || HasStunEffect;

    public EffectData CoverEffectData => EffectInfo?.Effects.FirstOrDefault(p => p.effectType == EffectType.Cover_50_Percent);

    public EffectData CoverFullDamageEffectData =>
        EffectInfo?.Effects.FirstOrDefault(p => p.effectType == EffectType.Cover_100_Percent);

    public EffectData DragonArmorEffectData =>
        EffectInfo?.Effects.FirstOrDefault(p => p.effectType == EffectType.DragonArmor);

    public ShieldEffect ShieldEffectData => (ShieldEffect)EffectInfo?.Effects.FirstOrDefault(p => p.effectType == EffectType.Shield);

    // Action
    public event EventHandler<int> OnHpChanged;
    public event EventHandler<int> OnMpChanged;

    public event EventHandler<float> OnShieldChanged;
    public event EventHandler<int> OnMoveAmount;
    public event EventHandler<int> OnNewRound;
    public event EventHandler<int> OnReduceHp;

    private SkillConfig SkillConfig { get; set; }
    public Character Character { get; set; }

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
            Character.HandleDeath();
        }
        else
        {
            bool isCrit = damageTakenParams.IsHitCritical || (CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive());
            Character.ShowMessage($"{-damage}", isCrit);
            OnHpChanged?.Invoke(this, -damageTakenParams.Damage);
        }
    }

    private void TryCover_PhamCuChich_Skill3(ref int damage)
    {
        if (EffectInfo.Effects.Any(p => p.effectType == EffectType.Cover_PhamCuChich_Skill3))
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

    public void HandleDamageTaken(int damage, Character character)
    {
        if (damage == 0) return;
        HandleHpChanged(ref damage, character);
        if (IsDie)
        {
            Character.HandleDeath();
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
            var shieldValue = ShieldEffectData.value;
            if (shieldValue >= damage)
            {
                ShieldEffectData.value -= damage;
                HandleShieldChange(character);
                hp = 0;
            }
            else
            {
                var remainingDamage = damage - shieldValue;
                HandleShieldChange(character);
                ShieldEffectData.value = 0;
                hp = -remainingDamage;
                HandleBreakShield(1, ShieldEffectData.damage);
            }
        }

        CurrentHp += hp;
        if (hp < 0)
            OnReduceHp?.Invoke(this, -hp);
        if (character != null)
            character.Info.SetDamageDealtInCurrentRound(-hp);
        CurrentHp = Math.Max(0, CurrentHp);
    }

    private void SetDamageDealtInCurrentRound(int damage)
    {
        DamageDealtInCurrentRound += damage;
        TryApplyLifeStealEffect();
        Debug.Log($"[{Character.characterConfig.characterName}] Sát thương đã gây ra trong round = {DamageDealtInCurrentRound}");
    }

    private void TryApplyLifeStealEffect()
    {
        var effect = EffectInfo.Effects.FirstOrDefault(p => p.effectType == EffectType.LifeSteal);
        if (effect is RollEffectData rollEffectData)
        {
            var heal = Roll.RollDice(rollEffectData.rollData);
            CurrentHp += heal;
            CurrentHp = Mathf.Min(CurrentHp, Character.GetMaxHp());
            OnHpChangedInvoke(heal);
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}]: Hút máu = {rollEffectData.rollData.rollTime}d{rollEffectData.rollData.rollValue} + {rollEffectData.rollData.add} = {heal}");
        }
    }

    private void HandleShieldChange(Character character)
    {
        int remainder = 0;

        if (ShieldAmount < 0)
        {
            remainder = -ShieldAmount;
        }

        OnShieldChanged?.Invoke(this, ShieldAmount * 1f / character.GetMaxHp());

        if (remainder > 0)
        {
            HandleDamageTaken(-remainder, character);
        }
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
        // Kiểm tra hiệu ứng SetDefToZero trước
        if (EffectInfo.Effects.Any(p => p.effectType == EffectType.SetDefToZero))
        {
            AlkawaDebug.Log(ELogCategory.EFFECT, 
                $"[{Character.characterConfig.characterName}] có hiệu ứng SetDefToZero => DEF = 0");
            return 0;
        }
        
        // Kiểm tra hiệu ứng ReduceStat_PhamCuChich_Skill3
        if (EffectInfo.Effects.Any(p => p.effectType == EffectType.ReduceStat_PhamCuChich_Skill3))
        {
            return 0;
        }

        int add = 0;
        foreach (var effect in EffectInfo.Effects)
        {
            if (effect is ChangeStatEffect changeStatEffect)
            {
                if (effect.effectType == EffectType.IncreaseDef)
                {
                    add += changeStatEffect.value;
                }
            }
        }
        return Attributes.def + add;
    }

    public int GetSpd()
    {
        int add = 0;
        foreach (var effect in EffectInfo.Effects)
        {
            if (effect is ChangeStatEffect changeStatEffect)
            {
                if (effect.effectType == EffectType.IncreaseSpd)
                {
                    add += changeStatEffect.value;
                }
            }
        }
        return Attributes.spd + add;
    }

    private bool IsImmobilized()
    {
        return EffectInfo.Effects
            .Any(effect => effect.effectType == EffectType.Immobilize);
    }

    private void OnEndTurn(object sender, EventArgs e)
    {
        RemoveAllEffect(EffectType.BlockSkill);
        DamageDealtInCurrentRound = 0;
    }

    private int CalculateBaseMovement()
    {
#if UNITY_EDITOR
        var baseValue = Attributes.overrideMaxMoveRange ? Attributes.maxMoveRangeOverride - MoveAmount : Attributes.maxMoveRange - MoveAmount;
#else
        var baseValue = Attributes.maxMoveRange - MoveAmount;
#endif
        var moveBuff = EffectInfo.Effects
            .Where(e => e.effectType == EffectType.IncreaseMoveRange)
            .Sum(e => ((ChangeStatEffect)e).value);
        return baseValue + moveBuff;
    }

    public int GetChiDef()
    {
        var chiDef = Attributes.chiDef;
        var buff = EffectInfo.Effects
            .Where(e => e.effectType == EffectType.ReduceChiDef)
            .Sum(e => ((ChangeStatEffect)e).value);
        return chiDef - buff;
    }

    private int CalculateMoveReduction()
    {
        var totalReduction = 0;

        foreach (var effect in EffectInfo.Effects)
        {
            switch (effect.effectType)
            {
                case EffectType.ReduceMoveRange:
                    {
                        var reduction = Roll.RollDice(1, 4, 0);
                        AlkawaDebug.Log(
                            ELogCategory.EFFECT,
                            $"Apply debuff {effect.effectType}: giảm di chuyển = 1d4 = {reduction}"
                        );
                        totalReduction += reduction;
                        break;
                    }
                case EffectType.ThietNhan_ReduceMoveRange when effect is ChangeStatEffect changeStatEffect:
                    totalReduction += changeStatEffect.value;
                    AlkawaDebug.Log(
                        ELogCategory.EFFECT,
                        $"Apply debuff {effect.effectType}: giảm di chuyển = {changeStatEffect.value}"
                    );
                    break;
            }
        }

        return totalReduction;
    }

    public void ResetBuffAfter()
    {
        foreach (var effect in EffectInfo.Effects.ToList()
                     .Where(effect => EffectInfo.TriggerAtEnd.Contains(effect.effectType)))
        {
            effect.duration--;
            if (effect.duration != 0) continue;
            AlkawaDebug.Log(ELogCategory.EFFECT, $"[{Character.characterConfig.characterName}] Removed effect: {effect.effectType}");
            switch (effect.effectType)
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
                case EffectType.PoisonousBloodPool:
                    RemovePoisonousBloodPool(effect);
                    break;
                case EffectType.RedDahlia:
                case EffectType.WhiteLotus:
                case EffectType.Marigold:
                case EffectType.NightCactus:
                    CheckFlowerEffectRemoved(effect);
                    break;
            }
            EffectInfo.Effects.Remove(effect);
        }
    }

    private void CheckFlowerEffectRemoved(EffectData flowerEffect)
    {
        // Kiểm tra xem nhân vật có hiệu ứng độc trùng không
        var venomousParasiteEffects = EffectInfo.Effects
            .Where(e => e.effectType == EffectType.VenomousParasite)
            .Cast<VenomousParasiteEffect>()
            .ToList();

        if (venomousParasiteEffects.Count > 0)
        {
            // Nếu có độc trùng, kích hoạt PoisonousBloodPool
            AlkawaDebug.Log(ELogCategory.EFFECT, 
                $"[{Character.characterConfig.characterName}] Hoa biến mất, kích hoạt độc trùng");

            // Tìm hiệu ứng độc trùng
            var parasiteEffect = venomousParasiteEffects.First();
            
            // Lấy loại hoa gốc
            var sourceFlower = flowerEffect.effectType;

            flowerEffect.Actor.Info.ApplyEffects(
                new List<EffectData>()
                {
                    new PoisonousBloodPoolEffect()
                    {
                        effectType = EffectType.PoisonousBloodPool,
                        duration = 2, 
                        Actor = flowerEffect.Actor, // Actor là người tạo ra hoa
                        impacts = GameplayManager.Instance.MapManager
                            .GetAllHexagonInRange(Character.Info.Cell, 1)
                            .ToList(),
                        effects = new List<EffectData>(), // Hiệu ứng nhiễm độc sẽ được xác định trong DamageTaken
                        sourceFlowerType = sourceFlower // Lưu loại hoa gốc
                    }
                }
            );

            // Giảm số lượng độc trùng
            parasiteEffect.value--;
            
            // Nếu độc trùng giảm về 0, xóa hiệu ứng
            if (parasiteEffect.value <= 0)
            {
                EffectInfo.Effects.Remove(parasiteEffect);
                AlkawaDebug.Log(ELogCategory.EFFECT, 
                    $"[{Character.characterConfig.characterName}] Độc trùng đã hết");
            }
        }
    }

    public void ResetBuffBefore()
    {
        OnNewRound?.Invoke(this, RoundIndex);
        
        // Chỉ reset MoveAmount nếu không phải lần đầu tiên sau khi load
        if (!IsFirstRoundAfterLoad)
        {
            MoveAmount = 0;
        }
        else
        {
            // Đánh dấu là đã xử lý lần đầu sau khi load
            IsFirstRoundAfterLoad = false;
            Debug.Log($"[{Character.characterConfig.characterName}] Giữ nguyên MoveAmount = {MoveAmount} từ save");
        }
        
        foreach (var effect in EffectInfo.Effects.ToList()
                     .Where(effect => EffectInfo.TriggerAtStart.Contains(effect.effectType)))
        {
            effect.duration--;
            if (effect.duration != 0) continue;
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Removed effect: {effect.effectType}");
            EffectInfo.Effects.Remove(effect);
        }

        HandleEffectCleanse();
        HandleApplyEffectBeforeNewRound();
        RoundIndex++;
    }

    public SkillInfo GetSkillInfo(int index, SkillTurnType skillTurnType)
    {
        var originalSkillInfo = SkillConfig.SkillConfigs[skillTurnType][index];
        
        // Thêm kiểm tra Blind
        if (EffectInfo.Effects.Any(p => p.effectType == EffectType.Blind))
        {
            // Tạo bản sao thủ công
            var modifiedSkillInfo = new SkillInfo
            {
                icon = originalSkillInfo.icon,
                name = originalSkillInfo.name,
                skillIndex = originalSkillInfo.skillIndex,
                mpCost = originalSkillInfo.mpCost,
                damageDescription = originalSkillInfo.damageDescription,
                range = Mathf.Max(1, originalSkillInfo.range - 1), // Giảm tầm skill đi 1, tối thiểu là 1
                directionType = originalSkillInfo.directionType,
                isDirectionalSkill = originalSkillInfo.isDirectionalSkill,
                canOverrideSetTargetCharacters = originalSkillInfo.canOverrideSetTargetCharacters,
                canBlockDamage = originalSkillInfo.canBlockDamage,
                damageType = originalSkillInfo.damageType,
                description = originalSkillInfo.description,
                canBeDodged = originalSkillInfo.canBeDodged
            };
            AlkawaDebug.Log(ELogCategory.EFFECT, $"[{Character.characterConfig.characterName}] bị Mù => Tầm sử dụng kỹ năng [{originalSkillInfo.name}] giảm từ {originalSkillInfo.range} xuống {modifiedSkillInfo.range}");
            return modifiedSkillInfo;
        }
        
        return originalSkillInfo;
    }

    public bool CanCastSkill(SkillInfo skillInfo)
    {
        return (CurrentMp >= skillInfo.mpCost || skillInfo.mpCost == 0) && HasEnoughActionPoints;
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
                foreach (int ap in effect.actionPoints)
                {
                    combined.Add(ap);
                }
            }

            combined.Reverse();

            int skipCount = 0;
            foreach (var effect in EffectInfo.Effects)
            {
                if (effect.effectType == EffectType.ThietNhan_BlockAP)
                {
                    skipCount++;
                }
                else if (effect.effectType == EffectType.ReduceAP && effect is ChangeStatEffect changeStatEffect)
                {
                    skipCount += changeStatEffect.value;
                }
                else if (effect.effectType == EffectType.CanSat_TakeAP)
                {
                    skipCount += 1;
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
            if (effect is ActionPointEffect { effectType: EffectType.IncreaseActionPoints } ape)
            {
                yield return ape;
            }
        }
    }


    public void IncreaseActionPointsValue()
    {
        IncreasePoints(ActionPoints);

        foreach (var effect in GetActionPointEffects())
        {
            IncreasePoints(effect.actionPoints);
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
            TryApplyBleedEffectAP(pointsToReduce);
            RemoveAllEffect(EffectType.IncreaseActionPoints);
            return;
        }

        return;

        bool TryReducePoints(IEnumerable<ActionPointEffect> effects)
        {
            foreach (var effect in effects)
            {
                for (var i = 0; i < effect.actionPoints.Count; i++)
                {
                    if (effect.actionPoints[i] != 3) continue;
                    effect.actionPoints[i] -= pointsToReduce;
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
#if UNITY_EDITOR
        var damage = Attributes.overrideDamage ? Attributes.atkOverride : Attributes.atk;
#else 
        var damage = Attributes.atk;
#endif
        foreach (var effectData in EffectInfo.Effects)
        {
            if (effectData.effectType == EffectType.IncreaseDamage && effectData is ChangeStatEffect changeStatEffect)
            {
                damage += changeStatEffect.value;
            }
        }

        return damage;
    }

    public void OnDamageTaken(DamageTakenParams damageTakenParams)
    {
        ApplyEffects(damageTakenParams.Effects);
        HandleDamageTaken(damageTakenParams);
        Character.HandleMpChanged(-damageTakenParams.ReducedMana);
    }

    public void OnMpChangedInvoke(int value)
    {
        OnMpChanged?.Invoke(this, value);
    }

    public void OnHpChangedInvoke(int value)
    {
        OnHpChanged?.Invoke(this, value);
    }

    public void CheckEffectAfterReceiveDamage(DamageTakenParams damageTakenParams)
    {
        // remove all sleep
        if (damageTakenParams.Effects.All(p => p.effectType != EffectType.Sleep))
        {
            if (EffectInfo.Effects.Any(p => p.effectType == EffectType.Sleep))
            {
                EffectInfo.Effects.RemoveAll(p => p.effectType == EffectType.Sleep);
                AlkawaDebug.Log(ELogCategory.EFFECT,
                    $"[{Character.characterConfig.characterName}] nhận damage => removed effect: Sleep");
            }
        }
    }

    private void HandleEffectCleanse()
    {
        foreach (var item in EffectInfo.Effects.ToList())
        {
            if (!EffectInfo.AppliedEffect.TryGetValue(item.effectType, out var value) || EffectInfo.DontRemoveEffect.Contains(item.effectType)) continue;
            var effectCleanse = _roll.GetEffectCleanse();
            var baseEffectCleanse = value.Item2;
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"{item.effectType}: Giải hiệu ứng = {effectCleanse} - Quy ước: {baseEffectCleanse}");
#if !ALWAY_APPLY_EFFECT
            if (effectCleanse < baseEffectCleanse) continue;
            EffectInfo.Effects.Remove(item);
#endif
            AlkawaDebug.Log(ELogCategory.EFFECT, $"[{Character.characterConfig.characterName}] Removed effect: {item.effectType}");
        }
    }

    private void HandleApplyEffectBeforeNewRound()
    {
        foreach (var item in EffectInfo.Effects.ToList())
        {
            switch (item.effectType)
            {
                case EffectType.Poison:
                case EffectType.ThietNhan_Poison:
                    {
                        if (item is RollEffectData poisonEffectData)
                        {
                            var rollData = poisonEffectData.rollData;
                            var damage = Roll.RollDice(rollData);
                            HandleDamageTaken(-damage, poisonEffectData.Actor);
                            Debug.Log(
                                $"[{Character.characterConfig.characterName}] - {item.effectType}: Damage = {rollData.rollTime}d{rollData.rollValue} + {rollData.add} = {damage}");
                        }

                        break;
                    }
            }
        }

        if (MustEndTurn)
        {
            Debug.Log(
                $"[{Character.characterConfig.characterName}] sleep = {HasSleepEffect} or stun = {HasStunEffect} => End turn");
            GameplayManager.Instance.HandleEndTurn("Có debuff sleep hoặc stun");
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
            if (_effectHandlers.TryGetValue(effect.effectType, out var handler))
            {
                handler(effect);
            }
            else
            {
                AlkawaDebug.Log(ELogCategory.EFFECT,
                    $"[{Character.characterConfig.characterName}] No handler for effect: {effect.effectType}");
            }
        }
    }

    private bool ShouldApplyEffect(EffectData effectData)
    {
        var effectResistance = _roll.GetEffectResistance();
        var baseEffectResistance = EffectInfo.AppliedEffect[effectData.effectType].Item1;
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"Debuff {effectData.effectType}: Kháng hiệu ứng = {effectResistance} - Quy ước: {baseEffectResistance}");
#if !ALWAY_APPLY_EFFECT
        return effectResistance < baseEffectResistance;
#else
        return true;
#endif
    }

    public void ApplyEffect(EffectData effectData)
    {
        EffectInfo.AddEffect(effectData);
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Added effect: {effectData.effectType.ToString()}");
    }

    private void ApplyIncreaseDamage(EffectData effectData)
    {
        if (effectData is not ChangeStatEffect changeStatEffect || changeStatEffect.value == 0)
            return;
        ApplyEffect(effectData);
        Character.ShowMessage($"Tăng {changeStatEffect.value} sát thương");
    }

    private void ApplyBlockSkill()
    {
        var effect = new EffectData { duration = 1, effectType = EffectType.BlockSkill };
        ApplyEffect(effect);
    }

    private void ApplySimpleEffect(EffectData effectData)
    {
        ApplyEffect(effectData);
    }

    private void ApplyBloodSealDamage(EffectData effectData)
    {
        if (EffectInfo.Effects.All(p => p.effectType != EffectType.BloodSealEffect))
            return;
        EffectInfo.Effects.RemoveAll(p => p.effectType == EffectType.BloodSealEffect);
        var hpDecreased = Character.GetMaxHp() - CurrentHp;
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
        RemoveAllEffect(effectData.effectType);
    }

    public void RemoveAllEffect(EffectType effectType)
    {
#if !ALWAY_APPLY_EFFECT
        var value = EffectInfo.Effects.RemoveAll(p => p.effectType == effectType);
        if (value <= 0) return;
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"[{Character.characterConfig.characterName}] Removed effect: {effectType.ToString()}");
#endif
    }

    public void RemoveEffect(EffectType effectType, Character actor)
    {
        for (int i = EffectInfo.Effects.Count - 1; i >= 0; i--)
        {
            var item = EffectInfo.Effects[i];
            if (item.effectType != effectType || item.Actor != actor) continue;
            EffectInfo.Effects.RemoveAt(i);
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Removed effect: {effectType}");
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
        if (effectData is VenomousParasiteEffect venomousParasiteEffect)
        {
            // Không còn cần xóa các hiệu ứng hoa ngay lập tức
            // Thay vào đó, lưu số lượng độc trùng vào VenomousParasiteEffect
            // Các hiệu ứng hoa sẽ tự động bị xóa sau DebuffRound và kích hoạt PoisonousBloodPool
            ApplySimpleEffect(effectData);
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Đã nhiễm {venomousParasiteEffect.value} độc trùng");
        }
        else if (effectData is ChangeStatEffect oldVersionEffect)
        {
            // Để tương thích với code cũ nếu vẫn còn sử dụng ChangeStatEffect
            var venomousParasite = new VenomousParasiteEffect
            {
                effectType = EffectType.VenomousParasite,
                value = oldVersionEffect.value,
                duration = -1,
                associatedFlowers = oldVersionEffect.value,
                Actor = oldVersionEffect.Actor
            };
            ApplySimpleEffect(venomousParasite);
            AlkawaDebug.Log(ELogCategory.EFFECT,
                $"[{Character.characterConfig.characterName}] Đã nhiễm {oldVersionEffect.value} độc trùng (từ phiên bản cũ)");
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
            ShieldEffectData.duration = changeStatEffect.duration;
            ShieldEffectData.value += changeStatEffect.value;
            ShieldEffectData.damage = Mathf.Max(ShieldEffectData.damage, changeStatEffect.damage);
        }

        if (ShieldEffectData != null)
        {
            Debug.Log($"Nhận lượng shield = {changeStatEffect} | lương shield hiện tại = {ShieldEffectData.value}");
            Debug.Log($"damage khi nổ shield = {ShieldEffectData.damage}");
            HandleShieldChange(Character);
        }
    }

    private void ApplyPoisonousBloodPool(EffectData effectData)
    {
        ApplySimpleEffect(effectData);
        if (effectData is PoisonousBloodPoolEffect poisonousBloodPoolEffect)
        {
            foreach (var cell in poisonousBloodPoolEffect.impacts)
            {
                cell.poisonousBloodPool.enabled = true;
            }
        }
    }

    private void RemovePoisonousBloodPool(EffectData effectData)
    {
        if (effectData is PoisonousBloodPoolEffect poisonousBloodPoolEffect)
        {
            foreach (var cell in poisonousBloodPoolEffect.impacts)
            {
                cell.poisonousBloodPool.enabled = false;
            }
        }
    }

    public int GetPoisonPowder()
    {
        return EffectInfo.Effects.Count(p => p.effectType == EffectType.PoisonPowder);
    }

    public int GetVenomousParasite()
    {
        var parasiteEffect = EffectInfo.Effects
            .OfType<VenomousParasiteEffect>()
            .FirstOrDefault();
        
        return parasiteEffect?.value ?? 0;
    }

    public int CountFlower()
    {
        return EffectInfo.Effects.Count(effect =>
            effect.effectType is EffectType.RedDahlia or EffectType.WhiteLotus or EffectType.Marigold
                or EffectType.NightCactus);
    }

    private bool IsVenomousEffect(EffectType effectType) =>
        effectType is EffectType.RedDahlia or EffectType.Marigold or EffectType.NightCactus or EffectType.WhiteLotus;

    public void TryApplyBleedEffectWithMove(int moveRange)
    {
        var effect = EffectInfo.Effects.FirstOrDefault(effect => effect.effectType == EffectType.Bleed);
        if (effect is not BleedEffect bleedEffect) return;
        var damage = Utils.RoundNumber(moveRange * 1f / bleedEffect.move);
        AlkawaDebug.Log(ELogCategory.EFFECT, $"Thất Ca ngâm gây {moveRange}/3 = {damage} lên {Character.characterConfig.characterName}");
        HandleDamageTaken(-damage, effect.Actor);
    }

    public void TryApplyBleedEffectAP(int ap)
    {
        var effect = EffectInfo.Effects.FirstOrDefault(effect => effect.effectType == EffectType.Bleed);
        if (effect is not BleedEffect bleedEffect) return;
        var damage = ap * 2;
        AlkawaDebug.Log(ELogCategory.EFFECT, $"Thất Ca ngâm gây {ap} * 2 = {damage} lên {Character.characterConfig.characterName}");
        HandleDamageTaken(-damage, effect.Actor);
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

    // Thêm phương thức để khởi tạo lại các hiệu ứng visual sau khi load
    public void InitializeEffectVisuals()
    {
        foreach (var effect in EffectInfo.Effects)
        {
            switch (effect.effectType)
            {
                case EffectType.Drunk:
                    // TODO: Khôi phục hiệu ứng visual cho drunk
                    break;
                case EffectType.Shield:
                    // Cập nhật hiển thị shield
                    OnShieldChanged?.Invoke(this, ShieldAmount * 1f / Character.GetMaxHp());
                    break;
                case EffectType.Poison:
                case EffectType.ThietNhan_Poison:
                    // TODO: Khôi phục hiệu ứng visual cho poison
                    break;
                // Thêm các hiệu ứng khác cần khôi phục visual
            }
        }

        // Xử lý riêng cho Hoắc Liên Hương shield
        if (Character is HoacLienHuong hlh && hlh.CurrentShield != null)
        {
            hlh.CurrentShield.SetShield(Character.Type, 3);
        }
    }

    public void UpdateShieldVisual()
    {
        if (ShieldEffectData != null)
        {
            OnShieldChanged?.Invoke(this, ShieldAmount * 1f / Character.GetMaxHp());
        }
    }

    public void RemoveAllFlowerEffects()
    {
        EffectInfo.Effects.RemoveAll(effect => IsVenomousEffect(effect.effectType));
    }
}