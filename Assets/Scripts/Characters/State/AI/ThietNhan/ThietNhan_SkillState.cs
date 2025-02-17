using System.Collections.Generic;

public class ThietNhan_SkillState : AISkillState
{
    public ThietNhan_SkillState(Character character) : base(character)
    {
    }
    
    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 0);
        var realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: Skill Damage = 1d4 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: damage = {baseDamage} + {skillDamage} = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects =  new Dictionary<EffectType, int>()
            {
                { EffectType.Poison, EffectConfig.DebuffRound}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 0);
        var realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: Skill Damage = 1d4 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: damage = {baseDamage} + {skillDamage} = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects =  new Dictionary<EffectType, int>()
            {
                { EffectType.Poison, EffectConfig.DebuffRound}
            }
        };
    }
}