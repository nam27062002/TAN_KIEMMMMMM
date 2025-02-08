public class LVDSkillState : SkillState
{
    public LVDSkillState(Character character) : base(character)
    {
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn()
    {
        var baseDamage = GetBaseDamage();
        var realDamage = (int)(1.5f * baseDamage);
        var reducedMana = (int)(0.5f * realDamage);
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{Character.characterConfig.characterName}] Vấn Truy Lưu: damage = {realDamage} | reduced Mana = {reducedMana}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            ReducedMana = reducedMana,
        };
    }
}