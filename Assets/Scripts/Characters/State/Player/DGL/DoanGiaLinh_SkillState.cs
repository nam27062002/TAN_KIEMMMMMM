using System.Collections.Generic;

public class DoanGiaLinh_SkillState : SkillState
{
    public DoanGiaLinh_SkillState(Character character) : base(character)
    {
    }

    //===================== SKILL 1 =====================

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn()
    {
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Nhiên Huyết");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BlockSkill, 0 },
                { EffectType.IncreaseActionPoints, 1 },
                { EffectType.IncreaseMoveRange, 2 },
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn()
    {
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Mộng Yểm");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.Immobilize, EffectConfig.DebuffRound },
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn()
    {
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Băng Hoại");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.ReduceMoveRange, 1 },
            }
        };
    }

    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn()
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 2);
        var realDamage = baseDamage + skillDamage;
        var increaseDamage = Utils.RoundNumber(Info.CurrentHp * 1f / 10);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Thược Dược Đỏ: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Thược Dược Đỏ: damage = {baseDamage} + {skillDamage} = {realDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Apply buff 6: tăng damage = {Info.CurrentHp} / 10 = {increaseDamage}");

        Info.OnDamageTaken(new DamageTakenParams()
        {
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.IncreaseDamage, increaseDamage},
            }
        });
        
        return new DamageTakenParams()
        {
            Damage = realDamage,
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn() =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn()
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 2);
        var realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Cúc Vạn Thọ: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Cúc Vạn Thọ: damage = {baseDamage} + {skillDamage} = {realDamage}");
        
        return new DamageTakenParams()
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.Stun , 0},
            }
        };
    }
}
