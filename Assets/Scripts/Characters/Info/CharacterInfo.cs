using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

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

    private List<int> ActionPoints { get; set; } = new() { 3, 3, 3 };

    // Buff & Debuff

    private EffectInfo EffectInfo { get; } = new();

    public bool IsLockSkill => EffectInfo.Effects.Any(effect => effect.EffectType == EffectType.BlockSkill);

    // Action
    public event EventHandler<int> OnHpChanged;
    public event EventHandler<int> OnMpChanged;
    public event EventHandler<int> OnMoveAmount;

    public SkillConfig SkillConfig { get; set; }
    public Character Character { get; set; }

    //
    private void HandleHpChanged(int value)
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
            Character.ShowMessage($"{-value}");
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
        return Attributes.maxMoveRange - MoveAmount + EffectInfo.Effects
            .Where(effect => effect.EffectType == EffectType.IncreaseMoveRange)
            .Sum(effect => ((ChangeStatEffect)effect).Value);
    }

    public void ResetBuffAfter()
    {
        ResetActionPoints();
    }

    public void ResetBuffBefore()
    {
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

    public List<int> GetActionPoints()
    {
        var actionPoints = new List<int>(ActionPoints);
        foreach (var effect in EffectInfo.Effects.Where(effect => effect.EffectType == EffectType.IncreaseActionPoints))
        {
            if (effect is ActionPointEffect actionPointEffect)
            {
                actionPoints.AddRange(actionPointEffect.ActionPoints);
            }
        }
        return actionPoints;
    }

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
        ApplyEffect(damageTakenParams.Effects);
    }

    private void ApplyEffect(Dictionary<EffectType, int> effects)
    {
        if (effects.TryGetValue(EffectType.IncreaseDamage, out var damage))
        {
            ApplyIncreaseDamage(damage);
        }

        if (effects.TryGetValue(EffectType.BlockSkill, out var _))
        {
            ApplyBlockSkill();
        }

        if (effects.TryGetValue(EffectType.IncreaseMoveRange, out var moveRange))
        {
            ApplyIncreaseMoveRange(moveRange);
        }

        if (effects.TryGetValue(EffectType.IncreaseActionPoints, out var actionPoints))
        {
            ApplyIncreaseActionPoints(actionPoints);
        }
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

    private void ApplyBlockSkill()
    {
        EffectInfo.AddEffect(new EffectData()
        {
            Duration = 1,
            EffectType = EffectType.BlockSkill,
        });
    }

    private void ApplyIncreaseMoveRange(int moveRange)
    {
        EffectInfo.AddEffect(new ChangeStatEffect()
        {
            Value = moveRange,
            Duration = 1,
            EffectType = EffectType.IncreaseMoveRange,
        });
    }

    private void ApplyIncreaseActionPoints(int actionPoints)
    {
        var actionPoint = new List<int>();
        for (var i = 0; i < actionPoints; i++)
        {
            actionPoint.Add(3);
        }
        EffectInfo.AddEffect(new ActionPointEffect()
        {
            ActionPoints = actionPoint,
            Duration = 1,
            EffectType = EffectType.IncreaseActionPoints,
        });
    }

    #endregion

    #region Roll

    private readonly Roll _roll;
    public int BaseDamage => _roll.GetBaseDamage();
    public HitChangeParams HitChangeParams => _roll.GetHitChange();
    public int Dodge => _roll.GetDodge();

    public void SetSpeed()
    {
        Speed = _roll.GetSpeed();
    }

    #endregion
}