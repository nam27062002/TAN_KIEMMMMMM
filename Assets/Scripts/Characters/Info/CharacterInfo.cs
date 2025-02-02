using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class CharacterInfo
{
    [ShowInInspector, ReadOnly] public Cell Cell { get; set; }
    [ShowInInspector, ReadOnly] public int Speed { get; set; }
    [ShowInInspector, ReadOnly] public bool LockSkill { get; set; }
    [ShowInInspector, ReadOnly] public CharacterAttributes Attributes { get; set; }
    
    [ShowInInspector, ReadOnly] public int CurrentHP { get; set; }
    [ShowInInspector, ReadOnly] public int CurrentMP { get; set; }
    
    [ShowInInspector, ReadOnly] public int MoveAmount { get; set; }
    [ShowInInspector, ReadOnly] public int MoveBuff { get; set; } 
    [ShowInInspector, ReadOnly] public List<int> ActionPoints { get; set; } = new(){ 3, 3, 3};
    [ShowInInspector, ReadOnly] public List<Cell> MoveCells { get; set; } = new();
    [ShowInInspector, ReadOnly] public SkillInfo SkillInfo { get; set; }
    [ShowInInspector, ReadOnly] public bool IsReact {get; set;}
   
    // Action
    public event EventHandler OnHpChanged;
    public event EventHandler OnMpChanged;
    
    public SkillConfig SkillConfig { get; set; }
    public Character Character { get; set; }
    public int Dodge =>  Character.Roll.GetDodge();
    public int BaseDamage => Character.Roll.GetBaseDamage(Character.characterConfig.damageRollData);
    public void HandleHpChanged(int value)
    {
        CurrentHP += value;
        CurrentHP = math.max(0, CurrentHP);
        if (CurrentHP <= 0)
        {
            Character.OnDie();
        }
        else
        {
            Character.ChangeState(ECharacterState.DamageTaken);
            OnHpChanged?.Invoke(this, EventArgs.Empty);  
        }
    }

    public void HandleMpChanged(int value)
    {
        CurrentMP += value;
        OnMpChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public int GetMoveRange()
    {
        return Attributes.maxMoveRange + MoveBuff - MoveAmount;
    }

    public void ResetBuffAfter()
    {
        
    }
    
    public void ResetBuffBefore()
    {
        LockSkill = false;
        MoveAmount = 0;
    }

    public SkillInfo GetSkillInfo(int index, SkillType skillType)
    {
        return SkillConfig.SkillConfigs[skillType][index];
    }

    public void OnCastSkill(SkillInfo skillInfo)
    {
        SkillInfo = skillInfo;
        HandleMpChanged(-skillInfo.mpCost);
        Character.HandleCastSkill();
        ReduceActionPoints(GetActionPoints(Character.CharacterManager.GetSkillType()));
    }
    
    public bool CanCastSkill(SkillInfo skillInfo, SkillType skillType)
    {
        return Attributes.mana >= skillInfo.mpCost && IsEnoughActionPoints(skillType);
    }
    
    public bool IsEnoughActionPoints(SkillType skillType)
    {
        var actionPoints = GetActionPoints(skillType);
        return ActionPoints.Any(point => actionPoints < point);
    }
    
    private int GetActionPoints(SkillType skillType)
    {
        return Character.characterConfig.actionPoints[skillType];
    }
    
    private void ReduceActionPoints(int point)
    {
        for (int i = 0; i < ActionPoints.Count; i++)
        {
            if (ActionPoints[i] <= point) continue;
            ActionPoints[i] -= point;
            //AlkawaDebug.Log($"[Gameplay] {Character.characterConfig.characterName} reduced action point: {ActionPoints[i]}");
            break;

        }
    }
    
    public void IncreaseActionPoints()
    {
        for (int i = 0; i < ActionPoints.Count; i++)
        {
            ActionPoints[i] = Math.Min(ActionPoints[i] + 1, 3);
        }
    }

    public void OnDamageTaken(int damage)
    {
        var message = damage == 0 ? "Né" : damage.ToString();
        HandleHpChanged(-damage);
        ShowMessage(message);
    }

    public void ShowMessage(string message)
    {
        Character.hpBar.ShowMessage(message);
    }

    public void ApplyBuff(SkillInfo skillInfo)
    {
        if (skillInfo.buffType.HasFlag(BuffType.IncreaseMoveRange))
        {
            MoveBuff += 1;
        }
        if (skillInfo.buffType.HasFlag(BuffType.IncreaseActionPoints))
        {
            IncreaseActionPoints();
        }
        if (skillInfo.buffType.HasFlag(BuffType.BlockSkill))
        {
            LockSkill = true;
        }
        ShowMessage("Nhận Buff");
    }
}