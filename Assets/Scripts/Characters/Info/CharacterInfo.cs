using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

public class CharacterInfo
{
    public CharacterInfo(SkillConfig skillConfig, CharacterAttributes characterAttributes, Character character)
    {
        SkillConfig = skillConfig;
        Attributes = characterAttributes;
        Character = character;

        CurrentHp = characterAttributes.health;
        CurrentMp = characterAttributes.mana;
        _roll = new Roll(Attributes, Character.characterConfig.characterName);
    }
    
    // Cell
    public Cell Cell { get; set; }
    public HashSet<Cell> MoveRange { get; set; } = new();

    public HashSet<Cell> SkillRange { get; set; } = new();

    // Skill
    public SkillInfo SkillInfo { get; set; }

    // Character
    public HashSet<Character> CharactersInSkillRange { get; set; } = new();

    //
    public int Speed { get; set; }
    public CharacterAttributes Attributes { get; set; }

    public int CurrentHp { get; set; }
    public int CurrentMp { get; set; }

    public int MoveAmount { get; set; }

    public int MoveBuff { get; set; }

    public List<int> ActionPoints { get; set; } = new() { 3, 3, 3 };

    // Buff & Debuff
    public bool LockSkill { get; set; }

    // Action
    public event EventHandler OnHpChanged;
    public event EventHandler OnMpChanged;
    public event EventHandler<int> OnMoveAmount;

    public SkillConfig SkillConfig { get; set; }
    public Character Character { get; set; }
    public int Dodge => _roll.GetDodge();

    #region Roll

    private readonly Roll _roll;
    public int BaseDamage => _roll.GetBaseDamage();
    public HitChangeParams HitChangeParams => _roll.GetHitChange();
    #endregion

    //

    public void SetSpeed()
    {
        Speed = _roll.GetSpeed();
    }
    
    public void HandleHpChanged(int value)
    {
        CurrentHp += value;
        CurrentHp = math.max(0, CurrentHp);
        if (CurrentHp <= 0)
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
        CurrentMp += value;
        OnMpChanged?.Invoke(this, EventArgs.Empty);
    }

    public void HandleMoveAmountChanged(int value)
    {
        OnMoveAmount?.Invoke(this, value);
    }

    public int GetMoveRange()
    {
        return Attributes.maxMoveRange + MoveBuff - MoveAmount;
    }

    public void ResetBuffAfter()
    {
        ResetActionPoints();
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

    public void OnCastSkill(SkillInfo skillInfo, SkillIndex skillIndex)
    {
        Character.SetSkill(new SkillStateParams
        {
            skillInfo = skillInfo,
        });
        HandleReduceActionPoints(GetActionPoints(GameplayManager.Instance.GetSkillType(Character)));
    }

    public bool CanCastSkill(SkillInfo skillInfo)
    {
        return CurrentMp >= skillInfo.mpCost && IsEnoughActionPoints();
    }

    public void OnDamageTaken(int damage)
    {
        var message = damage == -1 ? "Né" : damage == 0 ? "Apply Debuff" : damage.ToString();
        if (damage == -1) damage = 0;
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
            HandleIncreaseSlotActionPoints();
        }

        if (skillInfo.buffType.HasFlag(BuffType.BlockSkill))
        {
            LockSkill = true;
        }

        ShowMessage("Nhận Buff");
    }

    #region Action Points

    public void HandleIncreaseSlotActionPoints()
    {
        ActionPoints.Add(3);
    }

    public void HandleIncreaseValueActionPoints()
    {
        for (int i = 0; i < ActionPoints.Count; i++)
        {
            ActionPoints[i] = Math.Min(ActionPoints[i] + 1, 3);
        }
    }

    private bool IsEnoughActionPoints()
    {
        return ActionPoints.Any(point => point == 3);
    }

    private int GetActionPoints(SkillType skillType)
    {
        return Character.characterConfig.actionPoints[skillType];
    }

    private void HandleReduceActionPoints(int point)
    {
        for (var i = 0; i < ActionPoints.Count; i++)
        {
            if (ActionPoints[i] <= point) continue;
            ActionPoints[i] -= point;
            break;
        }
    }

    private void ResetActionPoints()
    {
        while (ActionPoints.Count > 3)
        {
            ActionPoints.RemoveAt(ActionPoints.Count - 1);
        }

        while (ActionPoints.Count < 3)
        {
            ActionPoints.Add(3);
        }

        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"{Character.characterConfig.characterName} reset action points");
    }

    #endregion
}