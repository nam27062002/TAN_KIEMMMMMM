using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoanGiaLinh_SkillState : SkillState
{
    public DoanGiaLinh_SkillState(Character character) : base(character)
    {
    }

    public int GetVenomousParasite()
    {
        return ((DoanGiaLinh)Character).GetVenomousParasite();
    }

    public void SetVenomousParasite(int venomousParasite)
    {
        ((DoanGiaLinh)Character).SetVenomousParasite(venomousParasite);
    }

    private void ApplyPoisonPowder(Character target)
    {
        var allCharacters = new List<Character>(GpManager.Characters);
        allCharacters.Remove(Character);
        foreach (var other in allCharacters)
        {
            other.Info.OnDamageTaken(new DamageTakenParams
            {
                Effects = new List<EffectData>()
                {
                    new()
                    {
                        EffectType = EffectType.PoisonPowder,
                    },
                }
            });
        }
    }

    private int ApplyVenomousParasiteExtraDamage(Character target, int currentDamage, List<EffectData> effects)
    {
        int flower = target.Info.CountFlower();
        int venomousParasite = GetVenomousParasite();
        if (flower > 0 && venomousParasite > 0 && Character.Info.IsToggleOn)
        {
            int value = Mathf.Min(flower, venomousParasite);
            SetVenomousParasite(flower - value);
            effects.Add(new ChangeStatEffect()
            {
                EffectType = EffectType.VenomousParasite,
                Value = value,
                Duration = -1
            });
            int extraDamage = Roll.RollDice(1, 6, 0) * value;
            AlkawaDebug.Log(ELogCategory.SKILL,
                $"[{CharName}] Độc trùng ăn hoa: damage = {value} * 1d6 = {extraDamage}");
            return currentDamage + extraDamage;
        }

        return currentDamage;
    }

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Nhiên Huyết");
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new()
                {
                    EffectType = EffectType.BlockSkill,
                    Duration = 0,
                },
                new ActionPointEffect()
                {
                    EffectType = EffectType.IncreaseActionPoints,
                    ActionPoints = new List<int> { 3 },
                    Duration = 1,
                },
                new ChangeStatEffect()
                {
                    EffectType = EffectType.IncreaseMoveRange,
                    Value = 2,
                    Duration = 1,
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
                EffectType = EffectType.Immobilize,
                Duration = EffectConfig.DebuffRound,
                Actor = Character,
            },
            new()
            {
                EffectType = EffectType.NightCactus,
                Actor = Character,
            },
            new RollEffectData()
            {
                EffectType = EffectType.Poison,
                Duration = EffectConfig.DebuffRound,
                RollData = new RollData()
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
                EffectType = EffectType.ReduceMoveRange,
                Duration = 1,
                Actor = Character
            },
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
        int skillDamage = Roll.RollDice(1, 4, 2);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Thược Dược Đỏ: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Thược Dược Đỏ: damage = {baseDamage} + {skillDamage} = {realDamage}");
        var effects = new List<EffectData>()
        {
            new()
            {
                EffectType = EffectType.RedDahlia,
                Actor = Character
            },
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
        int skillDamage = Roll.RollDice(1, 4, 2);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Sen Trắng: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Sen Trắng: damage = {baseDamage} + {skillDamage} = {realDamage}");
        var effects = new List<EffectData>()
        {
            new()
            {
                EffectType = EffectType.WhiteLotus,
                Actor = Character
            },
            new()
            {
                EffectType = EffectType.Sleep,
                Duration = EffectConfig.DebuffRound,
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
        int skillDamage = Roll.RollDice(1, 4, 2);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Cúc Vạn Thọ: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Cúc Vạn Thọ: damage = {baseDamage} + {skillDamage} = {realDamage}");
        var effects = new List<EffectData>()
        {
            new()
            {
                EffectType = EffectType.Marigold,
                Actor = Character
            },
            new()
            {
                EffectType = EffectType.Sleep,
                Duration = EffectConfig.DebuffRound,
                Actor = Character
            },
            new()
            {
                EffectType = EffectType.Stun,
                Actor = Character,
                Duration = EffectConfig.DebuffRound,
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
        int skillDamage = Roll.RollDice(1, 4, 0);
        int stack = target.Info.GetPoisonPowder();
        int totalSkillDamage = skillDamage * stack;
        int realDamage = baseDamage + totalSkillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Tuyết Điểm Hồng Phấn: skill damage = 1d4 * {stack} = {totalSkillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Tuyết Điểm Hồng Phấn: damage = {baseDamage} + {totalSkillDamage} = {realDamage}");
        var effects = new List<EffectData>()
        {
            new ChangeStatEffect()
            {
                EffectType = EffectType.ReduceChiDef,
                Value = stack,
                Duration = EffectConfig.DebuffRound,
                Actor = Character
            },
            new()
            {
                EffectType = EffectType.RemoveAllPoisonPowder,
                Actor = Character
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

    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Hồng Ti");
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new()
                {
                    EffectType = EffectType.RemoveAllPoisonPowder,
                    Actor = Character
                },
                new BleedEffect()
                {
                    EffectType = EffectType.Bleed,
                    Actor = Character,
                    Duration = EffectConfig.DebuffRound,
                    move = 3,
                    ap = 2
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Kim Tước Mai");
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new()
                {
                    EffectType = EffectType.RemoveAllPoisonPowder,
                    Actor = Character
                },
                new RollEffectData()
                {
                    EffectType = EffectType.LifeSteal,
                    Actor = Character,
                    Duration = EffectConfig.BuffRound,
                    RollData = new RollData(1,6,0),
                }
            },
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
        var validCharacters = GameplayManager.Instance.MapManager
            .GetCharactersInRange(Character.Info.Cell, _skillStateParams.SkillInfo)
            .Where(c => c.Info.EffectInfo.Effects.Any(e => e.EffectType == EffectType.PoisonPowder));
        foreach (var character in validCharacters)
        {
            AddTargetCharacters(character);
        }
    }
}