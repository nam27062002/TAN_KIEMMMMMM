using System;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Title("Character Type")] public CharacterType characterType;

    [Title("Animation"), Space] [SerializeField]
    private CharacterAnimationData characterAnimationData;

    [Title("References")] public HpBar hpBar;
    public GameObject model;

    [Title("Settings"), Space(10)] public CharacterConfig characterConfig;
    public SkillConfig skillConfig;
    public List<PassiveSkill> passiveSkills;

    protected CharacterStateMachine StateMachine { get; set; }

    protected HashSet<PassiveSkill> PendingPassiveSkillsTrigger { get; set; } = new HashSet<PassiveSkill>();

    public Roll Roll { get; private set; }
    public CharacterInfo CharacterInfo;
    
    public bool IsMainCharacter => GpManager.MainCharacter == this;
    // protected function
    protected GameplayManager GpManager => GameplayManager.Instance;
    protected MapManager MapManager => GpManager.MapManager;
    
    // Public function
    public CharacterAnimationData AnimationData => characterAnimationData;
    
    // public vitual function
    public virtual Type Type => Type.None;
    public virtual bool CanEndTurn => false;
    
    private void Awake()
    {
        SetStateMachine();
    }

    protected abstract void SetStateMachine();

    public void Initialize(Cell cell)
    {
        CharacterInfo = new CharacterInfo
        {
            SkillConfig = skillConfig,
            Attributes = characterConfig.characterAttributes,
            CurrentHp = characterConfig.characterAttributes.health,
            CurrentMp = characterConfig.characterAttributes.mana,
            Character = this,
        };
        skillConfig.SetSkillConfigs();
        Roll = new Roll(CharacterInfo.Attributes);
        SetCell(cell);
        SetIdle();
        SetSpeed();
        CharacterInfo.OnHpChanged += OnHpChanged;
        OnHpChanged(null, null);
        ChangeState(ECharacterState.Idle);
        SetPassiveSkills();
    }

    public virtual void SetMainCharacter()
    {
        CharacterInfo.HandleIncreaseValueActionPoints();
        CharacterInfo.ResetBuffBefore();
    }

    private void SetPassiveSkills()
    {
        foreach (var item in passiveSkills)
        {
            item.RegisterEvents();
        }
    }

    #region Set States

    private void SetIdle()
    {
        ChangeState(ECharacterState.Idle);
    }

    public void SetSkill(SkillStateParams skillStateParams)
    {
        ChangeState(ECharacterState.Skill, skillStateParams);
    }
    
    public virtual void SetMovement(List<Cell> cells)
    {
        ChangeState(ECharacterState.Move, new MoveStateParams(cells));
    }

    public void ChangeState(ECharacterState newState, StateParams stateParams = null)
    {
        StateMachine.ChangeState(newState, stateParams);
    }
    
    #endregion

    #region Skills

    public List<SkillInfo> GetSkillInfos(SkillType skillType)
    {
        return skillConfig.SkillConfigs[skillType];
    }

    public void HandleCastSkill()
    {
        // CharacterInfo.OnCastSkill();
    }

    public void HandleSelectSkill()
    {
        
    }
    #endregion
    
    #region Sub
    
    public void SetCell(Cell cell)
    {
        CharacterInfo.Cell = cell;
        cell.OnCharacterRegister(this);
    }
    
    protected virtual void SetSpeed()
    {
        CharacterInfo.Speed = Roll.GetSpeed();
    }
    
    private void OnHpChanged(object sender, EventArgs e)
    {
        var currentHp = CharacterInfo.CurrentHp;
        var maxHp = CharacterInfo.Attributes.health;
        hpBar.SetValue(currentHp * 1f / maxHp, $"{currentHp} / {maxHp}");
    }
    
    public void ShowHpBar()
    {
        hpBar.gameObject.SetActiveIfNeeded(true);
    }

    public void HideHpBar()
    {
        hpBar.gameObject.SetActiveIfNeeded(false);
    }
    
    public virtual void OnSelected()
    {
        CharacterInfo.Cell.ShowFocus();
    }

    public virtual void OnUnSelected()
    {
        CharacterInfo.Cell.HideFocus();
        HideSkillRange();
        HideMoveRange();
    }

    public void ShowMoveRange()
    {
        CharacterInfo.MoveRange = MapManager.GetHexagonsInMoveRange(CharacterInfo.Cell, CharacterInfo.GetMoveRange());
        foreach (var item in CharacterInfo.MoveRange)
        {
            item.ShowMoveRange();
        }
    }
    
    public void HideMoveRange()
    {
        if (CharacterInfo.MoveRange == null || CharacterInfo.MoveRange.Count == 0) return;
        foreach (var item in CharacterInfo.MoveRange)
        {
            item.HideMoveRange();
        }
        CharacterInfo.MoveRange.Clear();
    }
    
    public void ShowSkillRange(Cell cell, int range)
    {
        CharacterInfo.SkillRange = MapManager.GetHexagonsInAttack(cell, range);
        foreach (var item in CharacterInfo.SkillRange)
        {
            item.ShowSkillRange();
        }
    }
    
    public void HideSkillRange()
    {
        if (CharacterInfo.SkillRange == null || CharacterInfo.SkillRange.Count == 0) return;
        foreach (var item in CharacterInfo.SkillRange)
        {
            item.HideSkillRange();
        }
        CharacterInfo.SkillRange.Clear();
    }
    #endregion
    
    public void DestroyCharacter()
    {
        Destroy(gameObject);
        foreach (var item in passiveSkills)
        {
            item.UnregisterEvents();
        }
    }

    public void OnDie()
    {
        CharacterInfo.Cell.CellType = CellType.Walkable;
        var index = GpManager.Characters.IndexOf(this);
        ((UI_Ingame)UIManager.Instance.CurrentMenu).OnCharacterDeath(index);
        GpManager.HandleCharacterDie(this);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (characterAnimationData == null)
        {
            characterAnimationData = GetComponentInChildren<CharacterAnimationData>();
        }

        if (model == null)
        {
            model = gameObject.FindChildByName("Model");
        }

        if (hpBar == null)
        {
            hpBar = GetComponentInChildren<HpBar>();
        }

        skillConfig.OnValidate();
    }
#endif
}