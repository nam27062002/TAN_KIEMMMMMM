using System.Collections.Generic;

public class CanSat_SkillState : AISkillState
{
    public CanSat_SkillState(Character character) : base(character)
    {
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    EffectType = EffectType.IncreaseDef,
                    Actor = Character,
                    Value = 4,
                    Duration = MAX_ROUND
                },
                new ChangeStatEffect()
                {
                    EffectType = EffectType.IncreaseSpd,
                    Actor = Character,
                    Value = 6,
                    Duration = MAX_ROUND
                }
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    EffectType = EffectType.IncreaseDamage,
                    Actor = Character,
                    Value = 2,
                    Duration = MAX_ROUND
                },
                new ChangeStatEffect()
                {
                    EffectType = EffectType.IncreaseSpd,
                    Actor = Character,
                    Value = 6,
                    Duration = MAX_ROUND
                }
            }
        };
    }
}