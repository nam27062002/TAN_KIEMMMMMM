using System.Collections.Generic;

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
        return new DamageTakenParams { Damage = GetBaseDamage() };
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
                    Damage = _skillStateParams.DamageTakenParams.Damage
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
}