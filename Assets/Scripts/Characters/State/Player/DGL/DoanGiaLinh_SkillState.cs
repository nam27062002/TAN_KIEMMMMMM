using System.Collections.Generic;
using System.Linq;

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

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
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

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
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

    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character character)
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
    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character)
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

    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn(Character character)
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

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character character)
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
    
    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character character)
    {
        ApplyPoisonPowder();
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 0);
        var stack = character.CharacterInfo.GetPoisonPowder();
        var realDamage = baseDamage + skillDamage * stack;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Tuyết Điểm Hồng Phấn: skill damage = 1d4 * {stack} = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Tuyết Điểm Hồng Phấn: damage = {baseDamage} + {skillDamage} = {realDamage}");
        
        return new DamageTakenParams()
        {
            Damage = realDamage,
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.ReduceChiDef , stack},
                { EffectType.RemoveAllPoisonPowder , 0}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character character)
    {
        ApplyPoisonPowder();
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Hồng Ti");
        
        return new DamageTakenParams()
        {
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.RemoveAllPoisonPowder , 0}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character character)
    {
        ApplyPoisonPowder();
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Kim Tước Mai");
        
        return new DamageTakenParams()
        {
            Effects = new Dictionary<EffectType, int>()
            {
                { EffectType.RemoveAllPoisonPowder , 0}
            }
        };
    }
    
    //===================== SKILL 4 =====================
    protected override void SetTargetCharacters_Skill4_MyTurn()
    {
        var validCharacters = GameplayManager.Instance.MapManager
            .GetCharactersInRange(Character.CharacterInfo.Cell, _skillStateParams.SkillInfo)
            .Where(character => character.CharacterInfo.EffectInfo.Effects
                .Any(effect => effect.EffectType == EffectType.PoisonPowder));

        foreach (var character in validCharacters)
        {
            AddTargetCharacters(character);
        }
    }
    
    protected override void SetTargetCharacters_Skill4_TeammateTurn()
    {
        var validCharacters = GameplayManager.Instance.MapManager
            .GetCharactersInRange(Character.CharacterInfo.Cell, _skillStateParams.SkillInfo)
            .Where(character => character.CharacterInfo.EffectInfo.Effects
                .Any(effect => effect.EffectType == EffectType.PoisonPowder));

        foreach (var character in validCharacters)
        {
            AddTargetCharacters(character);
        }
    }
    
    protected override void SetTargetCharacters_Skill4_EnemyTurn()
    {
        AddTargetCharacters(GpManager.MainCharacter);
    }
}
