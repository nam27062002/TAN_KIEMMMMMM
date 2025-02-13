using System.Collections.Generic;

public class DoanGiaLinh_SkillState : SkillState
{
    public DoanGiaLinh_SkillState(Character character) : base(character)
    {
    }

    private void ApplyPoisonPowder()
    {
        var allCharacters = new List<Character>(GpManager.Characters);
        allCharacters.Remove(Character);
        foreach (var item in allCharacters)
        {
            item.CharacterInfo.OnDamageTaken(new DamageTakenParams()
            {
                Effects = new Dictionary<EffectType, int>()
                {
                    { EffectType.PoisonPowder , 0},
                }
            });
        }
    }

    //===================== SKILL 1 =====================

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn()
    {
        ApplyPoisonPowder();
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
        ApplyPoisonPowder();
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Mộng Yểm");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.Immobilize, EffectConfig.DebuffRound },
                { EffectType.NightCactus , 0}
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn()
    {
        ApplyPoisonPowder();
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
        ApplyPoisonPowder();
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 2);
        var realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Thược Dược Đỏ: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Thược Dược Đỏ: damage = {baseDamage} + {skillDamage} = {realDamage}");
        
        return new DamageTakenParams()
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.RedDahlia, 0 },
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn()
    {
        ApplyPoisonPowder();
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 2);
        var realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Sen Trắng: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Sen Trắng: damage = {baseDamage} + {skillDamage} = {realDamage}");
        
        return new DamageTakenParams()
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.WhiteLotus, 0 },
                { EffectType.Sleep , 0},
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn()
    {
        ApplyPoisonPowder();
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
                { EffectType.Marigold , 0}
            }
        };
    }
}
