using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HoacLienHuong_SkillState : SkillState
{
    public HoacLienHuong_SkillState(Character character) : base(character)
    {
    }
    
    protected override void HandleCastSkill()
    {
        base.HandleCastSkill();
        if (_skillStateParams.SkillInfo.skillIndex == SkillIndex.ActiveSkill2)
        {
            MoveToCell(_skillStateParams.TargetCell, 0.5f);
        }
    }
    
    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var deathCount = GpManager.GetCharacterDeathInRange(Character, 10);
        var skillDamage = Roll.RollDice(1, 4, 0) + deathCount;
        var totalDamage = baseDamage + skillDamage;
        Debug.Log($"Số thi thể = {deathCount}");
        Debug.Log($"Skill Damage = 1d4 + {deathCount} = {skillDamage}");
        Debug.Log($"Total Damage = {baseDamage} + {skillDamage} = {totalDamage}");
        return new DamageTakenParams
        {
            Damage = totalDamage,
            ReceiveFromCharacter = Character
        };   
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character character)
    {
        var coveredBy = GpManager.GetNearestAlly(Character);
        var effects = new List<EffectData>();
        if (coveredBy != null)
        {
            effects.Add(new EffectData()
            {
                EffectType = EffectType.Cover_50_Percent,
                Duration = EffectConfig.DebuffRound,
                Actor = coveredBy,
            });
        }
        Debug.Log($"Liên kết với đồng minh gần nhất: {coveredBy.characterConfig.characterName}");
        return new DamageTakenParams()
        {
            Effects = effects,
            ReceiveFromCharacter = character
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        return new DamageTakenParams()
        {
            ReceiveFromCharacter = character
        };
    }
    
    //===================== SKILL 3 =====================

    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character)
    {
        var currentShield = ((HoacLienHuong)character).CurrentShield;
        if (currentShield != null)
        {
            currentShield.UnsetShieldImpact(3);
        }
        Info.Cell.SetShield(Character.Type, 3);
        ((HoacLienHuong)character).CurrentShield = Info.Cell;
        return new DamageTakenParams();
    }
    
    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn(Character character)
    {
        Character.Info.ApplyEffects(new List<EffectData>()
        {
            new()
            {
                EffectType = EffectType.DragonArmor,
                Actor = character,
            }
        });
        
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new()
                {
                    EffectType = EffectType.SnakeArmor,
   
                    Actor = Character,
                }
            },
            ReceiveFromCharacter = Character
        };
    }
    
    //===================== SKILL 4 =====================
    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = GetSkillDamage(new RollData(2, 4, 2));
        var totalDamage = GetTotalDamage(baseDamage, skillDamage);
        return new DamageTakenParams
        {
            Damage = totalDamage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    EffectType = EffectType.Disarm,
                    Duration = EffectConfig.DebuffRound,
                    Actor = Character
                }
            },
            ReceiveFromCharacter = Character
        };   
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character _)
    {
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new()
                {
                    EffectType = EffectType.Cover_100_Percent,
                    Duration = 1,
                    Actor = Character,
                }
            },
            ReceiveFromCharacter = Character
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character _)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = GetSkillDamage(new RollData(2, 4, 2));
        var totalDamage = GetTotalDamage(baseDamage, skillDamage);
        return new DamageTakenParams()
        {
            Damage = totalDamage,
        };
    }
    
    //===================== SKILL 2 =====================
    protected override void SetTargetCharacters_Skill2_MyTurn()
    {
        var path = GpManager.MapManager.FindShortestPath(Info.Cell, _skillStateParams.TargetCell);
        var targets = (from item in path where item.CellType == CellType.Character && item.Character.Type == Type.AI select item.Character).ToList();
        foreach (var item in targets)
        {
            AddTargetCharacters(item);
        }
    }

    protected override void SetTargetCharacters_Skill2_TeammateTurn()
    {
        AddTargetCharacters(Character);
    }
    
    //===================== SKILL 3 =====================
    protected override void SetTargetCharacters_Skill3_TeammateTurn()
    {
        var walkAbleCells = GpManager.MapManager.GetAllHexagonInRange(GpManager.MainCharacter.Info.Cell, 1);
        foreach (var item in walkAbleCells.Where(item => item.CellType == CellType.Walkable))
        {
            TeleportToCell(item);
            break;
        }
        AddTargetCharacters(GpManager.MainCharacter);
    }
    
    protected override void SetTargetCharacters_Skill3_MyTurn()
    {
        AddTargetCharacters(Character);
    }
    
    //===================== SKILL 4 =====================
    
    protected override void SetTargetCharacters_Skill4_MyTurn()
    {
        var nearestEnemy = GpManager.GetNearestEnemy(Character);
        if (nearestEnemy != null) AddTargetCharacters(nearestEnemy);
    }
    
    protected override void SetTargetCharacters_Skill4_EnemyTurn()
    {
        var focusEnemy = GpManager.MainCharacter;
        GpManager.SwapPlayers(Character, focusEnemy);
        var character =
            new List<Character>(GpManager.MapManager.GetCharactersInRange(Info.Cell, _skillStateParams.SkillInfo));

        foreach (var item in character)
        {
            AddTargetCharacters(item);
        }
    }
}        
