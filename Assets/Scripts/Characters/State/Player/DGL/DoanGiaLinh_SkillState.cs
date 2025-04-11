using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoanGiaLinh_SkillState : SkillState
{
    // Thêm biến để theo dõi việc đã áp dụng poison powder trong lượt kỹ năng hiện tại chưa
    private bool _hasPoisonPowderBeenApplied = false;

    public DoanGiaLinh_SkillState(Character character)
        : base(character) { }

    // Override phương thức OnEnter để reset flag khi bắt đầu một kỹ năng mới
    public override void OnEnter(StateParams stateParams = null)
    {
        // Reset flag khi bắt đầu kỹ năng mới
        _hasPoisonPowderBeenApplied = false;
        base.OnEnter(stateParams);
    }

    public int GetVenomousParasite()
    {
        return ((DoanGiaLinh)Character).GetVenomousParasite();
    }

    private void SetVenomousParasite(int venomousParasite)
    {
        ((DoanGiaLinh)Character).SetVenomousParasite(venomousParasite);
    }

    private void ApplyPoisonPowder(Character target)
    {
        // Nếu đã áp dụng poison powder trong lượt kỹ năng này, không làm gì cả
        if (_hasPoisonPowderBeenApplied)
            return;

        // Đánh dấu đã áp dụng
        _hasPoisonPowderBeenApplied = true;
        
        // Logic hiện tại
        var allCharacters = new List<Character>(GpManager.Characters);
        allCharacters.Remove(Character);
        foreach (var other in allCharacters)
        {
            other.Info.OnDamageTaken(
                new DamageTakenParams
                {
                    Effects = new List<EffectData>()
                    {
                        new() { effectType = EffectType.PoisonPowder, },
                    }
                }
            );
        }
        
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Áp dụng Độc Phấn lên tất cả nhân vật");
    }

    private int ApplyVenomousParasiteExtraDamage(
        Character target,
        int currentDamage,
        List<EffectData> effects
    )
    {
        int flower = target.Info.CountFlower();
        int venomousParasite = GetVenomousParasite();
        if (flower > 0 && venomousParasite > 0 && Character.Info.IsToggleOn)
        {
            // Tính toán sát thương phụ từ độc trùng ăn hoa, nhưng không áp dụng hiệu ứng 
            // (sẽ áp dụng sau trong HandleAfterDamageTakenFinish)
            int value = Mathf.Min(flower, venomousParasite);
            bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
            int rollTimes = Roll.GetActualRollTimes(1, isCrit);
            int extraDamage = Roll.RollDice(1, 6, 0, isCrit) * value;
            AlkawaDebug.Log(
                ELogCategory.SKILL,
                $"[{CharName}] Độc trùng ăn hoa (sát thương phụ): damage = {value} * {rollTimes}d6 = {extraDamage}"
            );
            return currentDamage + extraDamage;
        }

        return currentDamage;
    }

    // Thêm phương thức để trích xuất logic hút độc phấn và hồi máu
    private int DrainPoisonPowderAndHeal(Character target, string skillName)
    {
        int stack = target.Info.GetPoisonPowder();
        if (stack <= 0)
            return stack;

        int healAmount = Mathf.Max(1, stack); // Tối thiểu là 1
        Character.Info.CurrentHp += healAmount;
        Character.Info.CurrentHp = Mathf.Min(Character.Info.CurrentHp, Character.GetMaxHp());
        Character.Info.OnHpChangedInvoke(healAmount);
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] {skillName}: Hút {stack} độc phấn => Hồi {healAmount} máu"
        );

        return stack;
    }

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Nhiên Huyết");
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new() { effectType = EffectType.BlockSkill, duration = 0, },
                new ActionPointEffect()
                {
                    effectType = EffectType.IncreaseActionPoints,
                    actionPoints = new List<int> { 3 },
                    duration = 1,
                },
                new ChangeStatEffect()
                {
                    effectType = EffectType.IncreaseMoveRange,
                    value = 2,
                    duration = 1,
                },
            },
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Mộng Yểm");
        int damage = 0;
        var effects = new List<EffectData>()
        {
            new()
            {
                effectType = EffectType.Immobilize,
                duration = EffectConfig.DebuffRound,
                Actor = Character,
            },
            new() { effectType = EffectType.NightCactus, Actor = Character, duration = EffectConfig.DebuffRound },
            new RollEffectData()
            {
                effectType = EffectType.Poison,
                duration = EffectConfig.DebuffRound,
                rollData = new RollData()
                {
                    rollTime = 1,
                    rollValue = 4,
                    add = 0,
                },
                Actor = Character
            }
        };
        damage = ApplyVenomousParasiteExtraDamage(target, damage, effects);
        return new DamageTakenParams
        {
            Damage = damage,
            Effects = effects,
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Băng Hoại");
        int damage = 0;
        var effects = new List<EffectData>()
        {
            new()
            {
                effectType = EffectType.ReduceMoveRange,
                duration = 1,
                Actor = Character
            },
            new()
            {
                effectType = EffectType.Prone,
                duration = EffectConfig.DebuffRound,
                Actor = Character
            }
        };
        damage = ApplyVenomousParasiteExtraDamage(target, damage, effects);
        return new DamageTakenParams
        {
            Damage = damage,
            Effects = effects,
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        int baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        int skillDamage = Roll.RollDice(1, 4, 2, isCrit);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] Thược Dược Đỏ: skill damage = {rollTimes}d4 + 2 = {skillDamage}"
        );
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] Thược Dược Đỏ: damage = {baseDamage} + {skillDamage} = {realDamage}"
        );
        var effects = new List<EffectData>()
        {
            new() { effectType = EffectType.RedDahlia, Actor = Character, duration = EffectConfig.DebuffRound },
            new()
            {
                effectType = EffectType.Fear,
                duration = EffectConfig.DebuffRound,
                Actor = Character
            }
        };
        realDamage = ApplyVenomousParasiteExtraDamage(target, realDamage, effects);
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = effects,
            ReceiveFromCharacter = Character,
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn(Character target)
    {
        ApplyPoisonPowder(target);
        int baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        int skillDamage = Roll.RollDice(1, 4, 2, isCrit);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] Sen Trắng: skill damage = {rollTimes}d4 + 2 = {skillDamage}"
        );
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] Sen Trắng: damage = {baseDamage} + {skillDamage} = {realDamage}"
        );
        var effects = new List<EffectData>()
        {
            new() { effectType = EffectType.WhiteLotus, Actor = Character, duration = EffectConfig.DebuffRound },
            new()
            {
                effectType = EffectType.Sleep,
                duration = EffectConfig.DebuffRound,
                Actor = Character
            }
        };
        realDamage = ApplyVenomousParasiteExtraDamage(target, realDamage, effects);
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = effects,
            ReceiveFromCharacter = Character,
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        int baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        int skillDamage = Roll.RollDice(1, 4, 2, isCrit);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] Cúc Vạn Thọ: skill damage = {rollTimes}d4 + 2 = {skillDamage}"
        );
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] Cúc Vạn Thọ: damage = {baseDamage} + {skillDamage} = {realDamage}"
        );
        var effects = new List<EffectData>()
        {
            new() { effectType = EffectType.Marigold, Actor = Character, duration = EffectConfig.DebuffRound },
            new()
            {
                effectType = EffectType.Sleep,
                duration = EffectConfig.DebuffRound,
                Actor = Character
            },
            new()
            {
                effectType = EffectType.Stun,
                Actor = Character,
                duration = EffectConfig.DebuffRound,
            }
        };
        realDamage = ApplyVenomousParasiteExtraDamage(target, realDamage, effects);
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = effects,
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        int baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        int skillDamage = Roll.RollDice(1, 4, 0, isCrit);
    
        int stack = DrainPoisonPowderAndHeal(target, "Tuyết Điểm Hồng Phấn");

        int totalSkillDamage = skillDamage * stack;
        int realDamage = baseDamage + totalSkillDamage;

        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] Tuyết Điểm Hồng Phấn: skill damage = {rollTimes}d4 * {stack} = {totalSkillDamage}"
        );
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] Tuyết Điểm Hồng Phấn: damage = {baseDamage} + {totalSkillDamage} = {realDamage}"
        );

        var effects = new List<EffectData>()
        {
            new ChangeStatEffect()
            {
                effectType = EffectType.ReduceChiDef,
                value = stack,
                duration = EffectConfig.DebuffRound,
                Actor = Character
            },
            new() { effectType = EffectType.RemoveAllPoisonPowder, Actor = Character }
        };

        realDamage = ApplyVenomousParasiteExtraDamage(target, realDamage, effects);

        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = effects,
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Hồng Ti");

        // Hút độc phấn và hồi máu
        DrainPoisonPowderAndHeal(target, "Hồng Ti");

        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new() { effectType = EffectType.RemoveAllPoisonPowder, Actor = Character },
                new RollEffectData()
                {
                    effectType = EffectType.LifeSteal,
                    Actor = Character,
                    duration = EffectConfig.BuffRound,
                    rollData = new RollData(1, 6, 0),
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Kim Tước Mai");

        // Hút độc phấn và hồi máu
        DrainPoisonPowderAndHeal(target, "Kim Tước Mai");

        int damage = 0;
        var effects = new List<EffectData>()
        {
            new() { effectType = EffectType.RemoveAllPoisonPowder, Actor = Character },
        };

        // Áp dụng sát thương từ độc trùng ăn hoa
        damage = ApplyVenomousParasiteExtraDamage(target, damage, effects);

        return new DamageTakenParams
        {
            Damage = damage,
            Effects = effects,
            ReceiveFromCharacter = Character
        };
    }

    protected override void SetTargetCharacters_Skill4_MyTurn()
    {
        SetTargetCharactersForAllySkill4();
    }

    protected override void SetTargetCharacters_Skill4_TeammateTurn()
    {
        SetTargetCharactersForAllySkill4();
    }

    protected override void SetTargetCharacters_Skill4_EnemyTurn()
    {
        AddTargetCharacters(GpManager.MainCharacter);
    }

    private void SetTargetCharactersForAllySkill4()
    {
        var validCharacters = GameplayManager
            .Instance.MapManager.GetCharactersInRange(
                Character.Info.Cell,
                _skillStateParams.SkillInfo
            );
        foreach (var character in validCharacters)
        {
            AddTargetCharacters(character);
        }
    }

    private void CheckAndApplyVenomousParasite(Character target)
    {
        // Chỉ áp dụng nếu toggle độc trùng bật và có đối tượng
        if (!Character.Info.IsToggleOn || target == null)
            return;

        int flower = target.Info.CountFlower();
        int venomousParasite = GetVenomousParasite();
        
        if (flower > 0 && venomousParasite > 0)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, 
                $"[{CharName}] Check áp dụng độc trùng sau khi kỹ năng hoàn thành: Hoa={flower}, Độc trùng={venomousParasite}");
            
            int value = Mathf.Min(flower, venomousParasite);
            SetVenomousParasite(venomousParasite - value);
            
            AlkawaDebug.Log(ELogCategory.SKILL, 
                $"[{CharName}] Áp dụng {value} độc trùng lên {target.characterConfig.characterName}, Độc trùng còn lại: {GetVenomousParasite()}");
            
            // Chỉ áp dụng VenomousParasite, không tạo PoisonousBloodPool
            // PoisonousBloodPool sẽ được tạo khi hoa bị xóa sau 3 round
            target.Info.ApplyEffects(
                new List<EffectData>()
                {
                    new VenomousParasiteEffect()
                    {
                        effectType = EffectType.VenomousParasite,
                        value = value,
                        duration = -1,
                        associatedFlowers = value,
                        Actor = Character
                    }
                }
            );
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill1_MyTurn()
    {
        // Nhiên Huyết (Skill 2 MyTurn) không có đối tượng, không cần áp dụng độc trùng
    }

    protected override void HandleAfterDamageTakenFinish_Skill2_MyTurn()
    {
        // Không có đối tượng, không cần áp dụng độc trùng
    }

    protected override void HandleAfterDamageTakenFinish_Skill2_TeammateTurn()
    {
        // Băng Hoại - Áp dụng độc trùng vào đối tượng
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                CheckAndApplyVenomousParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill2_EnemyTurn()
    {
        // Mộng Yểm - Áp dụng độc trùng vào đối tượng
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                CheckAndApplyVenomousParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill3_MyTurn()
    {
        // Thược Dược Đỏ - Có thể áp dụng cả hoa và độc trùng cùng lúc
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                CheckAndApplyVenomousParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill3_TeammateTurn()
    {
        // Sen Trắng - Có thể áp dụng cả hoa và độc trùng cùng lúc
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                CheckAndApplyVenomousParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill3_EnemyTurn()
    {
        // Cúc Vạn Thọ - Có thể áp dụng cả hoa và độc trùng cùng lúc
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                CheckAndApplyVenomousParasite(target);
            }
        }
    }

    private void ActivateInstantBloom(Character target)
    {
        if (target == null)
            return;
            
        // Đếm số lượng hoa trên mục tiêu
        int flowerCount = target.Info.CountFlower();
        
        if (flowerCount <= 0)
            return;
            
        AlkawaDebug.Log(ELogCategory.SKILL, 
            $"[{CharName}] Skill 4: Kích hoạt nở hoa lập tức cho {target.characterConfig.characterName}, Số hoa: {flowerCount}");
            
        // Kiểm tra độc trùng
        var venomousParasiteEffects = target.Info.EffectInfo.Effects
            .Where(e => e.effectType == EffectType.VenomousParasite)
            .Cast<VenomousParasiteEffect>()
            .ToList();
            
        if (venomousParasiteEffects.Count > 0)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, 
                $"[{CharName}] Skill 4: {target.characterConfig.characterName} có độc trùng, kích hoạt PoisonousBloodPool");
                
            // Tạo PoisonousBloodPool
            target.Info.ApplyEffects(
                new List<EffectData>()
                {
                    new PoisonousBloodPoolEffect()
                    {
                        effectType = EffectType.PoisonousBloodPool,
                        duration = 2,
                        Actor = Character,
                        impacts = GpManager
                            .MapManager.GetAllHexagonInRange(target.Info.Cell, 1)
                            .ToList(),
                        effects = new List<EffectData>(),
                    }
                }
            );
            
            // Giảm độc trùng
            var parasiteEffect = venomousParasiteEffects.First();
            int reduceAmount = Mathf.Min(parasiteEffect.value, flowerCount);
            parasiteEffect.value -= reduceAmount;
            
            AlkawaDebug.Log(ELogCategory.SKILL, 
                $"[{CharName}] Skill 4: Giảm {reduceAmount} độc trùng trên {target.characterConfig.characterName}");
                
            // Nếu độc trùng giảm về 0, xóa hiệu ứng
            if (parasiteEffect.value <= 0)
            {
                target.Info.EffectInfo.Effects.Remove(parasiteEffect);
                AlkawaDebug.Log(ELogCategory.SKILL, 
                    $"[{CharName}] Skill 4: Độc trùng trên {target.characterConfig.characterName} đã hết");
            }
        }
        
        // Xóa tất cả hiệu ứng hoa
        for (int i = target.Info.EffectInfo.Effects.Count - 1; i >= 0; i--)
        {
            var effect = target.Info.EffectInfo.Effects[i];
            if (effect.effectType is EffectType.RedDahlia or EffectType.WhiteLotus 
                or EffectType.Marigold or EffectType.NightCactus)
            {
                target.Info.EffectInfo.Effects.RemoveAt(i);
                AlkawaDebug.Log(ELogCategory.SKILL, 
                    $"[{CharName}] Skill 4: Xóa hiệu ứng hoa {effect.effectType} trên {target.characterConfig.characterName}");
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_MyTurn()
    {
        // Tuyết Điểm Hồng Phấn - Kích hoạt nở hoa lập tức và áp dụng độc trùng
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                // Kích hoạt nở hoa lập tức
                ActivateInstantBloom(target);
                
                // Áp dụng độc trùng
                CheckAndApplyVenomousParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_TeammateTurn()
    {
        // Hồng Ti - Kích hoạt nở hoa lập tức và áp dụng độc trùng
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                // Kích hoạt nở hoa lập tức
                ActivateInstantBloom(target);
                
                // Áp dụng độc trùng
                CheckAndApplyVenomousParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_EnemyTurn()
    {
        // Kim Tước Mai - Kích hoạt nở hoa lập tức và áp dụng độc trùng
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                // Kích hoạt nở hoa lập tức
                ActivateInstantBloom(target);
                
                // Áp dụng độc trùng
                CheckAndApplyVenomousParasite(target);
            }
        }
    }
}
