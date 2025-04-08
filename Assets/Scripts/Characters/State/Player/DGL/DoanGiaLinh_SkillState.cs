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
            Info.ApplyEffects(
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
                        effects = effects,
                    }
                }
            );

            int value = Mathf.Min(flower, venomousParasite);
            SetVenomousParasite(venomousParasite - value);
            effects.Add(
                new ChangeStatEffect()
                {
                    effectType = EffectType.VenomousParasite,
                    value = value,
                    duration = -1
                }
            );
            bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
            int rollTimes = Roll.GetActualRollTimes(1, isCrit);
            int extraDamage = Roll.RollDice(1, 6, 0, isCrit) * value;
            AlkawaDebug.Log(
                ELogCategory.SKILL,
                $"[{CharName}] Độc trùng ăn hoa: damage = {value} * {rollTimes}d6 = {extraDamage}"
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
            new() { effectType = EffectType.NightCactus, Actor = Character, },
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
            new() { effectType = EffectType.RedDahlia, Actor = Character },
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
            new() { effectType = EffectType.WhiteLotus, Actor = Character },
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
            new() { effectType = EffectType.Marigold, Actor = Character },
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
}
