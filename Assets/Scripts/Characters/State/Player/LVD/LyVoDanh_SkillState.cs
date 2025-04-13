using System.Collections.Generic;
using System.Linq;

public class LyVoDanh_SkillState : SkillState
{
    public LyVoDanh_SkillState(Character character) : base(character)
    {
    }

    #region Skill

    //=====================SKILL 1=====================================
    protected override DamageTakenParams GetDamageParams_Skill1_MyTurn(Character character)
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage(),
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BreakBloodSealDamage,
                    Actor = Character
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill1_TeammateTurn(Character character)
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage(),
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BreakBloodSealDamage,
                    Actor = Character
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill1_EnemyTurn(Character character)
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage(),
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BreakBloodSealDamage,
                    Actor = Character
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        var skillDamage = (int)(Roll.RollDice(1, 6, 3, isCrit) * 1.5f);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Skill Damage = 1.5 * ({rollTimes}d6 + 3) = {skillDamage}");
        var realDamage = baseDamage + skillDamage;
        var reducedMana = (int)(0.5f * realDamage);
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Vấn Truy Lưu: damage = {baseDamage} + {skillDamage} = {realDamage} | reduced Mana = {reducedMana}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            ReducedMana = reducedMana,
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BloodSealEffect,
                    Actor = Character
                }
            },
            ReceiveFromCharacter = Character,
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character character)
    {
        var damageResult = CalculateSkill2Damage();
        var effects = CreateBaseEffects();
        ApplyReduceHitChangeEffect();
        return new DamageTakenParams
        {
            Damage = damageResult.totalDamage,
            Effects = effects,
            ReceiveFromCharacter = Character,
        };
    }

    private (int totalDamage, int reducedMana) CalculateSkill2Damage()
    {
        var baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        var skillDamage = (int)(Roll.RollDice(1, 6, 3, isCrit) * 1.5f);
        
        AlkawaDebug.Log(ELogCategory.SKILL, 
            $"Skill Damage = 1.5 * ({rollTimes}d6 + 3) = {skillDamage}");
        
        var totalDamage = baseDamage + skillDamage;
        var reducedMana = (int)(0.5f * totalDamage);
        
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Sạ Bất Kiến: damage = {baseDamage} + {skillDamage} = {totalDamage} | reduced Mana = {reducedMana}");
            
        return (totalDamage, reducedMana);
    }

    private List<EffectData> CreateBaseEffects()
    {
        return new List<EffectData>()
        {
            new()
            {
                effectType = EffectType.BloodSealEffect,
                Actor = Character
            },
            new()
            {
                effectType = EffectType.Stun,
                Actor = Character,
                duration = EffectConfig.DebuffRound,
            }
        };
    }

    private void ApplyReduceHitChangeEffect()
    {
        // Tạo hiệu ứng tăng tỉ lệ chí mạng
        var reduceHitChangeEffect = new ChangeStatEffect()
        {
            effectType = EffectType.ReduceHitChange,
            Actor = Character,
            duration = EffectConfig.BuffRound,
            value = 1,
        };

        // Áp dụng cho chủ lượt nếu khác với nhân vật hiện tại
        var mainCharacter = GpManager.MainCharacter;
        if (mainCharacter != null && mainCharacter != Character)
        {
            mainCharacter.Info.ApplyEffect(reduceHitChangeEffect);
            AlkawaDebug.Log(ELogCategory.EFFECT, 
                $"[{CharName}] Sạ Bất Kiến: Áp dụng tăng tỉ lệ chí mạng cho {mainCharacter.characterConfig.characterName}");
        }

        // Áp dụng cho bản thân
        Character.Info.ApplyEffect(reduceHitChangeEffect);
        AlkawaDebug.Log(ELogCategory.EFFECT, 
            $"[{CharName}] Sạ Bất Kiến: Áp dụng tăng tỉ lệ chí mạng cho bản thân");
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        var skillDamage = (int)(Roll.RollDice(1, 6, 3, isCrit) * 1.5f);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Skill Damage = 1.5 * ({rollTimes}d6 + 3) = {skillDamage}");
        var realDamage = baseDamage + skillDamage;
        var reducedMana = (int)(0.5f * realDamage);
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Huề Hồ Viên Du: damage = {baseDamage} + {skillDamage} = {realDamage} | reduced Mana = {reducedMana}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BloodSealEffect,
                    Actor = Character
                },
            },
            ReceiveFromCharacter = Character,
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character)
    {
        var increaseDamage = Utils.RoundNumber(Character.Info.CurrentHp * 1f / 10);
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Nhất Giang Yên Trúc: increase damage = {Character.Info.CurrentHp} * 10% = {increaseDamage}");
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    effectType = EffectType.IncreaseDamage,
                    value = increaseDamage,
                    duration = EffectConfig.BuffRound,
                    Actor = Character
                },
                new DrunkEffect()
                {
                    effectType = EffectType.Drunk,
                    Actor = Character,
                    duration = EffectConfig.BuffRound,
                    SleepWhileMiss = false,
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character character)
    {
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Mính Đính Quy Lai");
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new RollEffectData()
                {
                    effectType = EffectType.LifeSteal,
                    Actor = Character,
                    duration = EffectConfig.BuffRound,
                    rollData = new RollData(1,6,0),
                },
                new DrunkEffect()
                {
                    effectType = EffectType.Drunk,
                    Actor = Character,
                    duration = EffectConfig.BuffRound,
                    SleepWhileMiss = false,
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn(Character character)
    {
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Liên Vấn");
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    effectType = EffectType.IncreaseDef,
                    Actor = Character,
                    duration = EffectConfig.BuffRound,
                    value = 4,
                },

                new DrunkEffect()
                {
                    effectType = EffectType.Drunk,
                    Actor = Character,
                    duration = EffectConfig.BuffRound,
                    SleepWhileMiss = true,
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    //=====================SKILL 4=====================================
    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(2, isCrit);
        var rollDamage = Roll.RollDice(2, 4, 2, isCrit);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Thất ca Ngâm: damage {baseDamage} + {rollTimes}d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BreakBloodSealDamage,
                    Actor = Character
                },
                new BleedEffect()
                {
                    effectType = EffectType.Bleed,
                    Actor = Character,
                    duration = EffectConfig.DebuffRound,
                    ap = 2,
                    move = 3
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(2, isCrit);
        var rollDamage = Roll.RollDice(2, 4, 2, isCrit);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Bất Thành Danh: damage {baseDamage} + {rollTimes}d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BreakBloodSealDamage,
                    Actor = Character,
                },
                new()
                {
                    effectType = EffectType.SetDefToZero,
                    Actor = Character,
                    duration = EffectConfig.DebuffRound, 
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(2, isCrit);
        var rollDamage = Roll.RollDice(2, 4, 2, isCrit);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Thất Ca Ngâm: damage = {baseDamage} + {rollTimes}d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BreakBloodSealDamage,
                    Actor = Character,
                },
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
            },
            ReceiveFromCharacter = Character
        };
    }

    //===================== Passtive Skill =====================================
    protected override DamageTakenParams GetDamageParams_PassiveSkill2_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Toàn Phong: damage = {baseDamage}");
        return new DamageTakenParams
        {
            Damage = baseDamage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.BreakBloodSealDamage,
                    Actor = Character,
                }
            },
            ReceiveFromCharacter = character
        };
    }

    #endregion

    #region Targets

    protected override void SetTargetCharacters_Skill3_MyTurn()
    {
        AddTargetCharacters(Character);
    }

    protected override void SetTargetCharacters_Skill3_EnemyTurn()
    {
        AddTargetCharacters(Character);
    }

    protected override void SetTargetCharacters_Skill3_TeammateTurn()
    {
        AddTargetCharacters(Character);
    }

    protected override void SetTargetCharacters_Skill4_MyTurn()
    {
        var allCharacter = new HashSet<Character>(TargetCharacters);
        foreach (var item in allCharacter)
        {
            AddTargetCharacters(item);
        }
    }

    protected override void SetTargetCharacters_Skill4_TeammateTurn()
    {
        var allCharacter = new HashSet<Character>(TargetCharacters);
        foreach (var item in allCharacter)
        {
            AddTargetCharacters(item);
        }
    }

    protected override void SetTargetCharacters_Skill4_EnemyTurn()
    {
        var allCharacter = new HashSet<Character>(TargetCharacters);
        foreach (var item in allCharacter)
        {
            AddTargetCharacters(item);
        }
    }

    #endregion

    protected override void HandleApplyDamageOnEnemy(Character character)
    {
        base.HandleApplyDamageOnEnemy(character);
        if (Character.Info.EffectInfo.Effects.Any(p => p.effectType == EffectType.Drunk))
        {
            AlkawaDebug.Log(ELogCategory.EFFECT, $"{CharName} check say");
            character.Info.ApplyEffects(new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.Sleep,
                    duration = EffectConfig.DebuffRound,
                    Actor = Character
                }
            });
        }
    }
}