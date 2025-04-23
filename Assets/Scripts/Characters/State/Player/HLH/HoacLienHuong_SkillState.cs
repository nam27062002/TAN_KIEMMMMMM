using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HoacLienHuong_SkillState : SkillState
{
    private HoacLienHuong _hlhCharacter;
    
    public HoacLienHuong_SkillState(Character character) : base(character)
    {
        _hlhCharacter = character as HoacLienHuong;
    }

    protected override void HandleCastSkill()
    {
        // Reset flag when starting a new skill
        _hlhCharacter?.ResetSelfDamageFlag();
        
        base.HandleCastSkill();
        if (_skillStateParams.SkillInfo.skillIndex == SkillIndex.ActiveSkill2)
        {
            MoveToCell(_skillStateParams.TargetCell, 0.5f);
        }
    }

    // Override methods for attacking to deal damage to self if max is reached
    protected override void HandleApplyDamageOnEnemy(Character character)
    {
        base.HandleApplyDamageOnEnemy(character);
        
        // Check and deal damage to self if max is reached
        _hlhCharacter?.ApplySelfDamageIfMaxDamage();
    }

    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var deathCount = GpManager.GetCharacterDeathInRange(Character, 10);
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        var skillDamage = Roll.RollDice(1, 4, 0, isCrit) + deathCount;
        var totalDamage = baseDamage + skillDamage;
        Debug.Log($"Corpse count = {deathCount}");
        Debug.Log($"Skill Damage = {rollTimes}d4 + {deathCount} = {skillDamage}");
        Debug.Log($"Total Damage = {baseDamage} + {skillDamage} = {totalDamage}");
        return new DamageTakenParams
        {
            Damage = totalDamage,
            ReceiveFromCharacter = Character
        };
    }

    // Override the method for getting base damage to add additional damage and crit effect
    protected override int GetBaseDamage()
    {
        var baseDamage = base.GetBaseDamage();
        
        // Add additional damage from Empresses' Bowstrings
        if (_hlhCharacter != null)
        {
            int additionalDamage = _hlhCharacter.GetAdditionalDamage();
            baseDamage += additionalDamage;
            
            if (additionalDamage > 0)
            {
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Empresses' Bowstrings: Added {additionalDamage} base damage");
            }
            
            // If max is reached, chance to add 2d4 damage (considered as crit effect)
            if (_hlhCharacter.IsMaxAdditionalDamage())
            {
                // 20% chance to deal crit damage
                if (Random.value <= 0.2f)
                {
                    int critDamage = Roll.RollDice(2, 4, 0);
                    baseDamage += critDamage;
                    AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Empresses' Bowstrings maxed: Activated crit, added {critDamage} damage (2d4)");
                }
            }
        }
        
        return baseDamage;
    }

    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character character)
    {
        var coveredBy = GpManager.GetNearestAlly(Character);
        var effects = new List<EffectData>();
        if (coveredBy != null)
        {
            effects.Add(new EffectData()
            {
                effectType = EffectType.Cover_50_Percent,
                duration = EffectConfig.DebuffRound,
                Actor = coveredBy,
            });
        }
        Debug.Log($"Linked with nearest ally: {coveredBy.characterConfig.characterName}");
        return new DamageTakenParams()
        {
            Effects = effects,
            ReceiveFromCharacter = character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] {_skillStateParams.SkillInfo.name}");
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>
            {
                new()
                {
                    effectType = EffectType.Blind,
                    duration = EffectConfig.DebuffRound, // Use standard debuff time
                    Actor = Character // The effect is caused by Hoac Lien Huong
                }
            },
            ReceiveFromCharacter = Character // The person receiving the effect is 'character' (who attacked HLH)
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
        
        // Deal damage to self if max is reached
        _hlhCharacter?.ApplySelfDamageIfMaxDamage();
        
        return new DamageTakenParams();
    }

    protected override DamageTakenParams GetDamageParams_Skill3_TeammateTurn(Character character)
    {
        bool isCritDragon = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive(); 
        int shieldValueDragon = Roll.RollDice(2, 4, 0, isCritDragon);
        
        bool isCritSnake = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive(); 
        int shieldValueSnake = Roll.RollDice(2, 4, 0, isCritSnake);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] - {_skillStateParams.SkillInfo.name}: Added {shieldValueSnake} shield (2d4) to self");
        
        var shieldSnake = new ShieldEffect()
        {
            effectType = EffectType.Shield,
            value = shieldValueSnake,
            duration = EffectConfig.BuffRound,
            Actor = Character,
            OtherCharacter = Character,
        };
        
        var shieldDragon = new ShieldEffect()
        {
            effectType = EffectType.Shield,
            value = shieldValueDragon,
            duration = EffectConfig.BuffRound,
            Actor = character,
            OtherCharacter = character
        };
        
        Character.Info.ApplyEffects(new List<EffectData>()
        {
            new()
            {
                effectType = EffectType.DragonArmor,
                Actor = Character, 
                duration = EffectConfig.BuffRound 
            },
            shieldDragon
        });
        
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.SnakeArmor,
                    Actor = character, 
                    duration = EffectConfig.BuffRound 
                },
                shieldSnake
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character character)
    {
        var cell = character.GetBackCell();
        TeleportToCell(cell);
        return new DamageTakenParams();
    }

    //===================== SKILL 4 =====================
    protected override DamageTakenParams GetDamageParams_Skill4_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = GetSkillDamage(new RollData(2, 4, 2));
        var totalDamage = GetTotalDamage(baseDamage, skillDamage);
        
        // Deal damage to self if max is reached
        _hlhCharacter?.ApplySelfDamageIfMaxDamage();
        
        return new DamageTakenParams
        {
            Damage = totalDamage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.Disarm,
                    duration = EffectConfig.DebuffRound,
                    Actor = Character
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character character)
    {
        var walkAbleCells = GpManager.MapManager.GetAllHexagonInRange(character.Info.Cell, 1);
        foreach (var item in walkAbleCells.Where(item => item.CellType == CellType.Walkable))
        {
            TeleportToCell(item);
            break;
        }
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.Cover_100_Percent,
                    duration = 1,
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
    
    protected override void SetTargetCharacters_Skill2_EnemyTurn()
    {
        AddTargetCharacters(GpManager.MainCharacter);
    }

    //===================== SKILL 3 =====================
    protected override void SetTargetCharacters_Skill3_TeammateTurn()
    {
        AddTargetCharacters(GpManager.MainCharacter);
    }

    protected override void SetTargetCharacters_Skill3_MyTurn()
    {
        AddTargetCharacters(Character);
    }

    protected override void SetTargetCharacters_Skill3_EnemyTurn()
    {
        AddTargetCharacters(GpManager.MainCharacter);
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
            new List<Character>(GpManager.MapManager.GetCharactersInRange(Info.Cell, _skillStateParams.SkillInfo, 1));

        foreach (var item in character)
        {
            AddTargetCharacters(item);
        }
    }

    protected override DamageTakenParams GetDamageParams_Skill1_MyTurn(Character character)
    {
        var damageParams = base.GetDamageParams_Skill1_MyTurn(character);
        
        // Deal damage to self if max is reached
        _hlhCharacter?.ApplySelfDamageIfMaxDamage();
        
        return damageParams;
    }

    protected override void TeleportToCell(Cell cell)
    {
        if (cell == null)
        {
            Debug.LogWarning($"[{CharName}] TeleportToCell: Target Cell is null!");
            return;
        }

        // Call the method from the base class
        base.TeleportToCell(cell);
        
        // Update CurrentShield if exists
        if (_hlhCharacter != null && cell.mainShieldCell == cell)
        {
            _hlhCharacter.CurrentShield = cell;
            _hlhCharacter.CurrentShieldPosition = cell.CellPosition;
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] TeleportToCell: Updated CurrentShield = {cell.CellPosition}");
        }
        
        // Update MainCell if it's the main character
        if (Character.IsMainCharacter)
        {
            GpManager.SetMainCell(cell);
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] TeleportToCell: Updated MainCell = {cell.CellPosition}");
        }
    }
}
