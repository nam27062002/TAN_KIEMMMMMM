using UnityEngine;

public class LyVoDanh_SkillState : SkillState
{
    public LyVoDanh_SkillState(Character character) : base(character)
    {
    }

    #region Skill

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
    
    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn()
    {
        Debug.Log("HEHEHEHEHEHEHHEHE");
        return new DamageTakenParams
        {

        };
    }

    #endregion

    #region Targets
    
    protected override void SetTargetCharacters_Skill2_MyTurn()
    {
        TargetCharacters.Clear();
        TargetCharacters.Add(Character);
    }
    #endregion

}