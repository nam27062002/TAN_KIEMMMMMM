using System.Collections.Generic;
using UnityEngine;

public class PhamCuChich_SkillState : SkillState
{
    public PhamCuChich_SkillState(Character self) : base(self)
    {
    }

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        if (character.Type == Character.Type)
        {
            character.Info.EffectInfo.RemoveAllDebuffs();
            AlkawaDebug.Log(ELogCategory.EFFECT, $"[{CharName}] Đã xóa tất cả hiệu ứng bất lợi cho {character.characterConfig.characterName}");
        }
        
        var shield = GetShieldValue();
        var realShield = Utils.RoundNumber(shield * 1f / 2);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Mỗi nhân vật nhận {realShield} giáp");
        
        var effect = new List<EffectData>()
        {
            new ShieldEffect()
            {
                effectType = EffectType.Shield,
                duration = EffectConfig.BuffRound,
                value = realShield,
                damage = realShield,
                Actor = Character
            }
        };
        Character.Info.EffectInfo.RemoveAllDebuffs();
        AlkawaDebug.Log(ELogCategory.EFFECT, $"[{CharName}] Đã xóa tất cả hiệu ứng bất lợi cho bản thân");
        Info.ApplyEffects(effect);

        return new DamageTakenParams
        {
            Effects = effect,
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character character)
    {
        var damageDealt = character.Info.DamageDealtInCurrentRound;
        Debug.Log($"[{character.skillConfig}] Sát thương đã gây ra: {damageDealt} => shield nhận được = {damageDealt}");
        var shield = damageDealt;
        var effect = new List<EffectData>()
        {
            new ShieldEffect()
            {
                effectType = EffectType.Shield,
                duration = EffectConfig.BuffRound,
                value = shield,
                damage = 0,
                Actor = Character
            }
        };
        Info.ApplyEffects(effect);

        return new DamageTakenParams
        {
            Effects = effect,
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new ShieldEffect()
                {
                    effectType = EffectType.Shield,
                    duration = EffectConfig.BuffRound,
                    value = _skillStateParams.DamageTakenParams.Damage,
                    damage = _skillStateParams.DamageTakenParams.Damage,
                    Actor = Character
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character)
    {
        var effect = new List<EffectData>()
        {
            new()
            {
                effectType = EffectType.Taunt,
                duration = EffectConfig.DebuffRound,
                Actor = Character
            }
        };

        if (mainTargetCharacter.Contains(character))
        {
            effect.Add(new ChangeStatEffect()
            {
                effectType = EffectType.ReduceAP,
                duration = EffectConfig.DebuffRound,
                Actor = Character,
                value = 1
            });
        }
        return new DamageTakenParams
        {
            Effects = effect,
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn(Character character)
    {
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new ShieldEffect()
                {
                    effectType = EffectType.Shield,
                    duration = EffectConfig.BuffRound,
                    Actor = Character,
                    damage = 5,
                    value = 5
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character character)
    {
        if (_skillStateParams?.DamageTakenParams == null)
        {
            return new DamageTakenParams();
        }
        return new DamageTakenParams
        {
            Damage = _skillStateParams.DamageTakenParams.Damage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.Taunt,
                    duration = EffectConfig.DebuffRound,
                    Actor = Character
                },
                new()
                {
                    effectType = EffectType.Silence,
                    Actor = Character
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character character)
    {
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int d4RollTimes = Roll.GetActualRollTimes(3, isCrit);
        int d6RollTimes = Roll.GetActualRollTimes(3, isCrit);
        var damage = Roll.RollDice(3, 4, 0, isCrit) + 6 + Roll.RollDice(3, 6, 0, isCrit);
        Info.ApplyEffects(new List<EffectData>()
        {
            new ChangeStatEffect()
            {
                effectType = EffectType.IncreaseMoveRange,
                duration = 1,
                value = 4,
            }
        });
        Debug.Log($"Damage = {d4RollTimes}d4 + 6 + {d6RollTimes}d6 = {damage}");

        return new DamageTakenParams
        {
            Damage = damage,
            Effects = new List<EffectData>()
            {
                new ()
                {
                    effectType = EffectType.Fear,
                    duration = EffectConfig.DebuffRound,
                    Actor = Character
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character character)
    {
        Info.ApplyEffects(new List<EffectData>()
        {
            new ChangeStatEffect()
            {
                effectType = EffectType.ReduceStat_PhamCuChich_Skill3,
                value = 0,
                Actor = character
            }
        });
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    effectType = EffectType.Cover_PhamCuChich_Skill3,
                    duration = 2,
                    value = 1,
                    Actor = Character,
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character character)
    {
        if (((PhamCuChich)Character).CurrentShield != null)
        {
            ((PhamCuChich)Character).CurrentShield.UnSetMainProjectile();
        }
        Info.Cell.SetMainProjectile();
        ((PhamCuChich)Character).CurrentShield = Info.Cell;
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new BlockProjectile()
                {
                    effectType = EffectType.BlockProjectile,
                    duration = 3,
                    Actor = Character,
                    targetCell = Info.Cell
                }
            }
        };
    }

    protected override void SetTargetCharacters_Skill2_EnemyTurn()
    {
        AddTargetCharacters(Character);
    }

    private int GetShieldValue()
    {
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(2, isCrit);
        var shield = Roll.RollDice(2, 6, 4, isCrit);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Lá chắn = {rollTimes}d6 + 4 = {shield}");
        return shield;
    }

    protected override void SetTargetCharacters_Skill2_MyTurn()
    {
        var validCharacters = GameplayManager.Instance.MapManager.GetCharactersInRange(Character.Info.Cell, _skillStateParams.SkillInfo);
        foreach (var character in validCharacters)
        {
            AddTargetCharacters(character);
        }
    }

    protected override void SetTargetCharacters_Skill3_MyTurn()
    {
        RemoveAllTargetCharacters();
        var validCharacters = GameplayManager.Instance.MapManager.GetCharactersInRange(Character.Info.Cell, _skillStateParams.SkillInfo);
        foreach (var character in validCharacters)
        {
            AddTargetCharacters(character);
        }
    }

    protected override void SetTargetCharacters_Skill3_TeammateTurn()
    {
        var validCharacters = GameplayManager.Instance.MapManager.GetCharactersInRange(Character.Info.Cell, _skillStateParams.SkillInfo);
        foreach (var character in validCharacters)
        {
            AddTargetCharacters(character);
        }
    }

    protected override void SetTargetCharacters_Skill3_EnemyTurn()
    {
        AddTargetCharacters(GpManager.MainCharacter);
    }

    protected override void SetTargetCharacters_Skill4_TeammateTurn()
    {
        AddTargetCharacters(GpManager.MainCharacter);
    }

    protected override void SetTargetCharacters_Skill4_EnemyTurn()
    {
        AddTargetCharacters(Character);
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_MyTurn()
    {
        BreakShield(3);
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_TeammateTurn()
    {
        BreakShield(3);
    }

    protected override void HandleAfterDamageTakenFinish_Skill4_EnemyTurn()
    {
        BreakShield(3);
    }

    private void BreakShield(int range)
    {
        if (Character.Info.ShieldEffectData == null)
        {
            Debug.Log("Không có shield => không gây damage");
            return;
        }
        Character.Info.HandleBreakShield(range, Character.Info.ShieldEffectData.damage);
    }
}