using System.Collections.Generic;

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
        var increaseDamage = Character.CharacterInfo.CurrentHp / 10;
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{Character.characterConfig.characterName}] Nhất Giang Yên Trúc: increase damage = {Character.CharacterInfo.CurrentHp}*10% = {increaseDamage}");
        return new DamageTakenParams
        {
            IncreaseDamage = increaseDamage,
        };
    }
    //=====================SKILL 4=====================================
    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn()
    {
        var baseDamage = GetBaseDamage();
        var rollDamage = Roll.RollDice(2, 4, 2);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{Character.characterConfig.characterName}] Thất ca Ngâm: damage {baseDamage} + 2d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn()
    {
        var baseDamage = GetBaseDamage();
        var rollDamage = Roll.RollDice(2, 4, 2);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{Character.characterConfig.characterName}] Thất ca Ngâm: damage {baseDamage} + 2d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn()
    {
        var baseDamage = GetBaseDamage();
        var rollDamage = Roll.RollDice(2, 4, 2);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{Character.characterConfig.characterName}] Thất Ca Ngâm: damage = {baseDamage} + 2d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
        };
    }

    protected override DamageTakenParams GetDamageParams_PassiveSkill2_MyTurn()
    {
        var baseDamage = GetBaseDamage();
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{Character.characterConfig.characterName}] Toàn Phong: damage = {baseDamage}");
        return new DamageTakenParams
        {
            Damage = baseDamage,
        };
    }
    
    #endregion

    #region Targets
    
    protected override void SetTargetCharacters_Skill3_MyTurn()
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

}