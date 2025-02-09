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
        _roll = new Roll(this, Character.characterConfig.characterName);
    }
    
    // Cell
    public Cell Cell { get; set; }
    public HashSet<Cell> MoveRange { get; set; } = new();

    public HashSet<Cell> SkillRange { get; set; } = new();

    // Skill
    public SkillInfo SkillInfo { get; set; }

    // Character
    public HashSet<Character> CharactersInSkillRange { get; set; } = new();

    // Stat
    public int Speed { get; set; }
    public CharacterAttributes Attributes { get; set; }
    public int CurrentHp { get; set; }
    public int CurrentMp { get; set; }

    public int MoveAmount { get; set; }
    public int MoveBuff { get; set; }

    public List<int> ActionPoints { get; set; } = new() { 3, 3, 3 };

    // Buff & Debuff

    public EffectInfo EffectInfo { get; set; } = new();
    
    public bool LockSkill { get; set; }
    // Action
    public event EventHandler<int> OnHpChanged;
    public event EventHandler<int> OnMpChanged;
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
        if (value == 0) return;
        CurrentHp += value;
        CurrentHp = math.max(0, CurrentHp);
        if (CurrentHp <= 0)
        {
            Character.OnDie();
        }
        else
        {
            Character.ShowMessage(value.ToString());
            OnHpChanged?.Invoke(this, value);
        }
    }

    public void HandleMpChanged(int value)
    {
        if (value == 0) return;
        CurrentMp += value;
        OnMpChanged?.Invoke(this, value);
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

    public SkillInfo GetSkillInfo(int index, SkillTurnType skillTurnType)
    {
        return SkillConfig.SkillConfigs[skillTurnType][index];
    }
    
    public bool CanCastSkill(SkillInfo skillInfo)
    {
        return CurrentMp >= skillInfo.mpCost && IsEnoughActionPoints();
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

    private int GetActionPoints(SkillTurnType skillTurnType)
    {
        return Character.characterConfig.actionPoints[skillTurnType];
    }

    public void HandleReduceActionPoints()
    {
        var point = GetActionPoints(Character.GetSkillTurnType());
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

    #region Damage Logic

    public int GetCurrentDamage()
    {
        var damage = Attributes.atk;
        foreach (var effectData in EffectInfo.Effects)
        {
            if (effectData.EffectType == EffectType.IncreaseDamage && effectData is ChangeStatEffect changeStatEffect)
            {
                damage += changeStatEffect.Value;
            }
        }
        return damage;
    }
    
    public void OnDamageTaken(DamageTakenParams damageTakenParams)
    {
        HandleHpChanged(-damageTakenParams.Damage);
        HandleMpChanged(-damageTakenParams.ReducedMana);
        ApplyIncreaseDamage(damageTakenParams.IncreaseDamage);
    }

    private void ApplyIncreaseDamage(int damage)
    {
        if (damage == 0) return;
        EffectInfo.AddEffect(new ChangeStatEffect
        {
            Value = damage,
            Duration = EffectConfig.BuffRound,
            EffectType = EffectType.IncreaseDamage,
        });
        Character.ShowMessage($"Tăng {damage} sát thương");
    }
    #endregion
}