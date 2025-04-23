using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoanGiaLinh_SkillState : SkillState
{
    // Thêm biến để theo dõi việc đã áp dụng poison powder trong lượt kỹ năng hiện tại chưa
    private bool _hasPoisonPowderBeenApplied = false;

    public DoanGiaLinh_SkillState(Character self)
        : base(self) { }

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
        
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Applied Poison Powder to all characters");
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
            // Calculate additional damage from poison insects eating flowers, but do not apply effect
            // (will be applied later in HandleAfterDamageTakenFinish)
            int value = Mathf.Min(flower, venomousParasite);
            bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
            int rollTimes = Roll.GetActualRollTimes(1, isCrit);
            int extraDamage = Roll.RollDice(1, 6, 0, isCrit) * value;
            AlkawaDebug.Log(
                ELogCategory.SKILL,
                $"[{CharName}] Venomous Parasite consumes flower (extra damage): damage = {value} * {rollTimes}d6 = {extraDamage}"
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

        int healAmount = Mathf.Max(1, stack); // Minimum is 1
        Character.Info.CurrentHp += healAmount;
        Character.Info.CurrentHp = Mathf.Min(Character.Info.CurrentHp, Character.GetMaxHp());
        Character.Info.OnHpChangedInvoke(healAmount);
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] {skillName}: Drained {stack} poison powder => Healed {healAmount} HP"
        );

        return stack;
    }

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] {_skillStateParams.SkillInfo.name}");
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
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] {_skillStateParams.SkillInfo.name}");
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
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] {_skillStateParams.SkillInfo.name}");
        int damage = 0;
        var effects = new List<EffectData>()
        {
            new()
            {
                effectType = EffectType.ReduceMoveRange,
                duration = Roll.RollDice(1,4,0),
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
            $"[{CharName}] {_skillStateParams.SkillInfo.name}: skill damage = {rollTimes}d4 + 2 = {skillDamage}"
        );
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] {_skillStateParams.SkillInfo.name}: damage = {baseDamage} + {skillDamage} = {realDamage}"
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
            $"[{CharName}] {_skillStateParams.SkillInfo.name}: skill damage = {rollTimes}d4 + 2 = {skillDamage}"
        );
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] {_skillStateParams.SkillInfo.name}: damage = {baseDamage} + {skillDamage} = {realDamage}"
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
            $"[{CharName}] {_skillStateParams.SkillInfo.name}: skill damage = {rollTimes}d4 + 2 = {skillDamage}"
        );
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] {_skillStateParams.SkillInfo.name}: damage = {baseDamage} + {skillDamage} = {realDamage}"
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
    
        int stack = DrainPoisonPowderAndHeal(target, _skillStateParams.SkillInfo.name);

        int totalSkillDamage = skillDamage * stack;
        int realDamage = baseDamage + totalSkillDamage;

        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] {_skillStateParams.SkillInfo.name}: skill damage = {rollTimes}d4 * {stack} = {totalSkillDamage}"
        );
        AlkawaDebug.Log(
            ELogCategory.SKILL,
            $"[{CharName}] {_skillStateParams.SkillInfo.name}: damage = {baseDamage} + {totalSkillDamage} = {realDamage}"
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
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] {_skillStateParams.SkillInfo.name}");

        // Hút độc phấn và hồi máu
        DrainPoisonPowderAndHeal(target, _skillStateParams.SkillInfo.name);

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
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] {_skillStateParams.SkillInfo.name}");

        // Hút độc phấn và hồi máu
        DrainPoisonPowderAndHeal(target, _skillStateParams.SkillInfo.name);

        int damage = 0;
        var effects = new List<EffectData>()
        {
            new() { effectType = EffectType.RemoveAllPoisonPowder, Actor = Character },
        };

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

        AddTargetCharacters(Character);
    }

    private void CheckAndApplyVenomousParasite(Character target)
    {
        // Only apply if toggle is on and target exists
        if (!Character.Info.IsToggleOn || target == null)
            return;

        int flower = target.Info.CountFlower();
        int venomousParasite = GetVenomousParasite();
        
        if (flower > 0 && venomousParasite > 0)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, 
                $"[{CharName}] Check applying Venomous Parasite after skill finish: Flower={flower}, Parasite={venomousParasite}");
            
            int value = Mathf.Min(flower, venomousParasite);
            SetVenomousParasite(venomousParasite - value);
            
            AlkawaDebug.Log(ELogCategory.SKILL, 
                $"[{CharName}] Applied {value} Venomous Parasite to {target.characterConfig.characterName}, Remaining Parasites: {GetVenomousParasite()}");
            
            // Only apply VenomousParasite, not creating PoisonousBloodPool
            // PoisonousBloodPool will be created when flowers are removed after 3 rounds
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
        // Naturally Blood (Skill 2 MyTurn) has no target, no need to apply venomous parasite
    }

    protected override void HandleAfterDamageTakenFinish_Skill2_MyTurn()
    {
        // No target, no need to apply venomous parasite
    }

    protected override void HandleAfterDamageTakenFinish_Skill2_TeammateTurn()
    {
        // Ice Decay - Apply venomous parasite to target
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
        // Dream Suppression - Apply venomous parasite to target
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
        // Red Dahlia - Can apply both flower and venomous parasite simultaneously
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
        // White Lotus - Can apply both flower and venomous parasite simultaneously
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
        // Marigold - Can apply both flower and venomous parasite simultaneously
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                CheckAndApplyVenomousParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_MyTurn()
    {
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                ActivateFlowerAndParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_TeammateTurn()
    {
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                ActivateFlowerAndParasite(target);
            }
        }
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_EnemyTurn()
    {
        if (_skillStateParams.Targets != null && _skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                ActivateFlowerAndParasite(target);
            }
        }
    }

    private void ActivateFlowerAndParasite(Character target)
    {
        if (target == null) return;

        // Get flower type BEFORE removing
        EffectType sourceFlowerType = GetSourceFlowerTypeFromTarget(target);
        if (sourceFlowerType == EffectType.None) return; // No flowers to activate
        
        int flowerCount = target.Info.CountFlower(); // Count flowers before removing
        if (flowerCount <= 0) return; // Not really necessary because sourceFlowerType was already checked

        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Skill 4: Activated {flowerCount} flower(s) ({sourceFlowerType}) on {target.characterConfig.characterName}");

        // Remove all flower effects AFTER determining the type
        target.Info.RemoveAllFlowerEffects();

        // Check and activate venomous parasites
        CheckAndApplyVenomousParasite(target);
    }

    // Helper function to get the first flower type found on the target
    private EffectType GetSourceFlowerTypeFromTarget(Character target)
    {
        var flowerEffect = target.Info.EffectInfo.Effects.FirstOrDefault(e => 
            e.effectType == EffectType.RedDahlia || 
            e.effectType == EffectType.WhiteLotus || 
            e.effectType == EffectType.Marigold || 
            e.effectType == EffectType.NightCactus);
            
        return flowerEffect?.effectType ?? EffectType.None;
    }
}
