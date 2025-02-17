using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoanGiaLinh_SkillState : SkillState
{
    public DoanGiaLinh_SkillState(Character character) : base(character)
    {
    }

    public int GetVenomousParasite()
    {
        return ((DoanGiaLinh)Character).GetVenomousParasite();
    }

    public void SetVenomousParasite(int venomousParasite)
    {
        ((DoanGiaLinh)Character).SetVenomousParasite(venomousParasite);
    }
    
    private void ApplyPoisonPowder(Character target)
    {
        var allCharacters = new List<Character>(GpManager.Characters);
        allCharacters.Remove(Character);
        foreach (var other in allCharacters)
        {
            other.CharacterInfo.OnDamageTaken(new DamageTakenParams
            {
                Effects = new Dictionary<EffectType, int>
                {
                    { EffectType.PoisonPowder, 0 }
                }
            });
        }
    }

    private int ApplyVenomousParasiteExtraDamage(Character target, int currentDamage, Dictionary<EffectType, int> effects)
    {
        int flower = target.CharacterInfo.CountFlower();
        int venomousParasite = GetVenomousParasite();
        if (flower > 0 && venomousParasite > 0 && Character.CharacterInfo.IsToggleOn)
        {
            int value = Mathf.Min(flower, venomousParasite);
            SetVenomousParasite(flower - value);
            effects[EffectType.VenomousParasite] = value;
            int extraDamage = Roll.RollDice(1, 6, 0) * value;
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Độc trùng ăn hoa: damage = {value} * 1d6 = {extraDamage}");
            return currentDamage + extraDamage;
        }
        return currentDamage;
    }

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Nhiên Huyết");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>
            {
                { EffectType.BlockSkill, 0 },
                { EffectType.IncreaseActionPoints, 1 },
                { EffectType.IncreaseMoveRange, 2 }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Mộng Yểm");
        int damage = 0;
        var effects = new Dictionary<EffectType, int>
        {
            { EffectType.Immobilize, EffectConfig.DebuffRound },
            { EffectType.NightCactus, 0 }
        };
        damage = ApplyVenomousParasiteExtraDamage(target, damage, effects);
        return new DamageTakenParams { Damage = damage, Effects = effects };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Băng Hoại");
        int damage = 0;
        var effects = new Dictionary<EffectType, int>
        {
            { EffectType.ReduceMoveRange, 1 }
        };
        damage = ApplyVenomousParasiteExtraDamage(target, damage, effects);
        return new DamageTakenParams { Damage = damage, Effects = effects };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        int baseDamage = GetBaseDamage();
        int skillDamage = Roll.RollDice(1, 4, 2);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Thược Dược Đỏ: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Thược Dược Đỏ: damage = {baseDamage} + {skillDamage} = {realDamage}");
        var effects = new Dictionary<EffectType, int>
        {
            { EffectType.RedDahlia, 0 }
        };
        realDamage = ApplyVenomousParasiteExtraDamage(target, realDamage, effects);
        return new DamageTakenParams { Damage = realDamage, Effects = effects };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn(Character target)
    {
        ApplyPoisonPowder(target);
        int baseDamage = GetBaseDamage();
        int skillDamage = Roll.RollDice(1, 4, 2);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Sen Trắng: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Sen Trắng: damage = {baseDamage} + {skillDamage} = {realDamage}");
        var effects = new Dictionary<EffectType, int>
        {
            { EffectType.WhiteLotus, 0 },
            { EffectType.Sleep, 0 }
        };
        realDamage = ApplyVenomousParasiteExtraDamage(target, realDamage, effects);
        return new DamageTakenParams { Damage = realDamage, Effects = effects };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        int baseDamage = GetBaseDamage();
        int skillDamage = Roll.RollDice(1, 4, 2);
        int realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Cúc Vạn Thọ: skill damage = 1d4 + 2 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Cúc Vạn Thọ: damage = {baseDamage} + {skillDamage} = {realDamage}");
        var effects = new Dictionary<EffectType, int>
        {
            { EffectType.WhiteLotus, 0 },
            { EffectType.Sleep, 0 }
        };
        realDamage = ApplyVenomousParasiteExtraDamage(target, realDamage, effects);
        return new DamageTakenParams { Damage = realDamage, Effects = effects };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        int baseDamage = GetBaseDamage();
        int skillDamage = Roll.RollDice(1, 4, 0);
        int stack = target.CharacterInfo.GetPoisonPowder();
        int totalSkillDamage = skillDamage * stack;
        int realDamage = baseDamage + totalSkillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Tuyết Điểm Hồng Phấn: skill damage = 1d4 * {stack} = {totalSkillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Tuyết Điểm Hồng Phấn: damage = {baseDamage} + {totalSkillDamage} = {realDamage}");
        var effects = new Dictionary<EffectType, int>
        {
            { EffectType.ReduceChiDef, stack },
            { EffectType.RemoveAllPoisonPowder, 0 }
        };
        realDamage = ApplyVenomousParasiteExtraDamage(target, realDamage, effects);
        return new DamageTakenParams { Damage = realDamage, Effects = effects };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Hồng Ti");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>
            {
                { EffectType.RemoveAllPoisonPowder, 0 }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character target)
    {
        ApplyPoisonPowder(target);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Kim Tước Mai");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>
            {
                { EffectType.RemoveAllPoisonPowder, 0 }
            }
        };
    }

    protected override void SetTargetCharacters_Skill4_MyTurn()
    {
        SetTargetCharactersForAllySkill4();
    }

    protected override void SetTargetCharacters_Skill4_TeammateTurn()
    {
        SetTargetCharactersForAllySkill4();
    }

    protected override void SetTargetCharacters_Skill4_EnemyTurn()
    {
        AddTargetCharacters(GpManager.MainCharacter);
    }

    private void SetTargetCharactersForAllySkill4()
    {
        var validCharacters = GameplayManager.Instance.MapManager
            .GetCharactersInRange(Character.CharacterInfo.Cell, _skillStateParams.SkillInfo)
            .Where(c => c.CharacterInfo.EffectInfo.Effects.Any(e => e.EffectType == EffectType.PoisonPowder));
        foreach (var character in validCharacters)
        {
            AddTargetCharacters(character);
        }
    }
}
