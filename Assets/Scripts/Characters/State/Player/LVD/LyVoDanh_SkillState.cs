using System.Collections.Generic;

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
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BreakBloodSealDamage , 1}
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill1_TeammateTurn(Character character)
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage(),
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BreakBloodSealDamage , 1}
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill1_EnemyTurn(Character character)
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage(),
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BreakBloodSealDamage , 1}
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = (int)(Roll.RollDice(1,6, 3) * 1.5f);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Skill Damage = 1.5 * (1d6 + 3) = {skillDamage}");
        var realDamage = baseDamage + skillDamage;
        var reducedMana = (int)(0.5f * realDamage);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Vấn Truy Lưu: damage = {baseDamage} + {skillDamage} = {realDamage} | reduced Mana = {reducedMana}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            ReducedMana = reducedMana,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BloodSealEffect , 1}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = (int)(Roll.RollDice(1,6, 3) * 1.5f);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Skill Damage = 1.5 * 1d6 + 3 = {skillDamage}");
        var realDamage = baseDamage + skillDamage;
        var reducedMana = (int)(0.5f * realDamage);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Sạ Bất Kiến: damage = {baseDamage} + {skillDamage} = {realDamage} | reduced Mana = {reducedMana}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BloodSealEffect , 1}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = (int)(Roll.RollDice(1,6, 3) * 1.5f);
        AlkawaDebug.Log(ELogCategory.SKILL, $"Skill Damage = 1.5 * 1d6 + 3 = {skillDamage}");
        var realDamage = baseDamage + skillDamage;
        var reducedMana = (int)(0.5f * realDamage);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Huề Hồ Viên Du: damage = {baseDamage} + {skillDamage} = {realDamage} | reduced Mana = {reducedMana}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BloodSealEffect , 1}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character)
    {
        var increaseDamage = Character.CharacterInfo.CurrentHp / 10;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Nhất Giang Yên Trúc: increase damage = {Character.CharacterInfo.CurrentHp}*10% = {increaseDamage}");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>()
            {
                {EffectType.IncreaseDamage, increaseDamage },
            },
        };
    }
    //=====================SKILL 4=====================================
    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var rollDamage = Roll.RollDice(2, 4, 2);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Thất ca Ngâm: damage {baseDamage} + 2d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BreakBloodSealDamage , 1}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var rollDamage = Roll.RollDice(2, 4, 2);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Thất ca Ngâm: damage {baseDamage} + 2d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BreakBloodSealDamage , 1}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var rollDamage = Roll.RollDice(2, 4, 2);
        var realDamage = baseDamage + rollDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Thất Ca Ngâm: damage = {baseDamage} + 2d4 + 2 = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BreakBloodSealDamage , 1}
            }
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
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.BreakBloodSealDamage , 1}
            }
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