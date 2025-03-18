using System.Collections.Generic;
using UnityEngine;

public class PhamCuChich_SkillState : SkillState
{
    public PhamCuChich_SkillState(Character character) : base(character)
    {
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        var shield = GetShieldValue();
        var realShield = Utils.RoundNumber(shield * 1f / 2);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Mỗi nhân vật nhận {realShield} giáp");
        var effect = new List<EffectData>()
        {
            new ShieldEffect()
            {
                EffectType = EffectType.Shield,
                Duration = EffectConfig.BuffRound,
                Value = realShield,
                Damage = realShield,
                Actor = Character
            }
        };
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
                EffectType = EffectType.Shield,
                Duration = EffectConfig.BuffRound,
                Value = shield,
                Damage = 0,
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
                    EffectType = EffectType.Shield,
                    Duration = EffectConfig.BuffRound,
                    Value = _skillStateParams.DamageTakenParams.Damage,
                    Damage = _skillStateParams.DamageTakenParams.Damage,
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
                EffectType = EffectType.Taunt,
                Duration = EffectConfig.DebuffRound,
                Actor = Character
            }
        };

        if (mainTargetCharacter.Contains(character))
        {
            effect.Add(new ChangeStatEffect()
            {
                EffectType = EffectType.ReduceAP,
                Duration = EffectConfig.DebuffRound,
                Actor = Character,
                Value = 1
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
                    EffectType = EffectType.Shield,
                    Duration = EffectConfig.BuffRound,
                    Actor = Character, 
                    Damage = 5,
                    Value = 5
                }
            }
        };
    }
        
    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character character)
    {
        if (_skillStateParams == null || _skillStateParams.DamageTakenParams == null)
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
                    EffectType = EffectType.Taunt,
                    Duration = EffectConfig.DebuffRound,
                    Actor = Character
                },
                new()
                {
                    EffectType = EffectType.Silence,
                    Actor = Character
                }
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character character)
    {
        var damage = Roll.RollDice(3, 4, 0) + 6 + Roll.RollDice(3, 6, 0);
        Info.ApplyEffects(new List<EffectData>()
        {
            new ChangeStatEffect()
            {
                EffectType = EffectType.IncreaseMoveRange,
                Duration = 1,
                Value = 4,
            }
        });
        Debug.Log($"Damage =  3d4 + 6 + 3d6 = {damage}");
        
        return new DamageTakenParams
        {
            Damage = damage,
            Effects = new List<EffectData>()
            {
                new ()
                {
                    EffectType = EffectType.Fear,
                    Duration = EffectConfig.DebuffRound,
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
                EffectType = EffectType.ReduceStat_PhamCuChich_Skill3,
                Value = 0,
                Actor = character
            }
        });
        return new DamageTakenParams
        {
            Effects = new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    EffectType = EffectType.Cover_PhamCuChich_Skill3,
                    Duration = 2,
                    Value = 1,
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
                    EffectType = EffectType.BlockProjectile,
                    Duration = 3,
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
        var shield = Roll.RollDice(2, 6,4);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Lá chắn = 2d6 + 4 = {shield}");
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
        Character.Info.HandleBreakShield(range, Character.Info.ShieldEffectData.Damage);
    }
}