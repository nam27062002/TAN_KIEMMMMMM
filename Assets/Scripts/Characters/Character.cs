using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public virtual Type Type => Type.None;
    public CharacterType characterType;
    public CharacterAnimationData characterAnimationData;
    public HpBar hpBar;
    public GameObject model;
    public CharacterStateMachine StateMachine { get; set; }
    protected GameplayManager GpManager => GameplayManager.Instance;
    [Title("Settings"), Space(10)] 
    public CharacterConfig characterConfig;
    public CharacterInfo characterInfo;
    public SkillConfig skillConfig;
    
    public Roll Roll {get; set;}
    
    public virtual bool CanEndTurn => false;
    public bool IsMainCharacter => GpManager.MainCharacter == this;

    public ECharacterState CurrentState { get; set; } = ECharacterState.Idle;
    public ECharacterState PreviousState { get; set; } = ECharacterState.Idle;
    
    private void Awake()
    {
        SetStateMachine();
    }

    protected abstract void SetStateMachine();
    
    public void Initialize(Cell cell)
    {
        characterInfo = new CharacterInfo
        {
            SkillConfig = skillConfig,
            Attributes = characterConfig.characterAttributes,
            CurrentHp = characterConfig.characterAttributes.health,
            CurrentMp = characterConfig.characterAttributes.mana,
            Character = this,
        };
        skillConfig.SetSkillConfigs();
        Roll = new Roll(characterInfo.Attributes);
        SetCell(cell);
        SetCharacterPosition(cell);
        SetIdle();
        SetSpeed();
        characterInfo.OnHpChanged += OnHpChanged;
        OnHpChanged(null, null);
        ChangeState(ECharacterState.Idle);
    }

    public virtual void SetMainCharacter()
    {
        characterInfo.HandleIncreaseValueActionPoints();
        characterInfo.ResetBuffBefore();
    }

    #region Set States  
    
    private void SetIdle()
    {
        ChangeState(ECharacterState.Idle);
    }
    
    public void HandleCastSkill()
    {
        ChangeState(ECharacterState.Skill);
    }
    
    public void ChangeState(ECharacterState newState, StateParams stateParams = null)
    {
        PreviousState = CurrentState;
        CurrentState = newState;
        StateMachine.ChangeState(newState, stateParams);
    }
    
    public void PlayAnim(AnimationParameterNameType animationParameterNameType, Action onEndAnim = null)
    {
        characterAnimationData.PlayAnimation(animationParameterNameType, onEndAnim);
    }
    #endregion
    
    public void SetCell(Cell cell)
    {
        characterInfo.Cell = cell;
        cell.OnCharacterRegister(this);
    }

    private void SetCharacterPosition(Cell cell)
    {
        var pos = cell.transform.position;
        pos.y += characterConfig.characterHeight / 2f;
        transform.position = pos;
    }
    
    private void OnHpChanged(object sender, EventArgs e)
    {
        var currentHp = characterInfo.CurrentHp;
        var maxHp = characterInfo.Attributes.health;
        hpBar.SetValue(currentHp * 1f/ maxHp, $"{currentHp} / {maxHp}");
    }

    public List<SkillInfo> GetSkillInfos(SkillType skillType)
    {
        return skillConfig.SkillConfigs[skillType];
    }

    public virtual void MoveCharacter(List<Cell> cells)
    {
        ChangeState(ECharacterState.Move, new MoveStateParams(cells));   
    }

    public void MoveCharacter(Vector3 targetPos, float duration)
    {
        PlayAnim(targetPos.x > transform.position.x ? AnimationParameterNameType.MoveRight : AnimationParameterNameType.MoveLeft);
        transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
    }
    public virtual void OnSelected()
    {
        characterInfo.Cell.ShowFocus();
    }
    
    public virtual void OnUnSelected()
    {
        characterInfo.Cell.HideFocus();
    }

    protected virtual void SetSpeed()
    {
        characterInfo.Speed = Roll.GetSpeed();
    }

    public void ShowHpBar()
    {
        hpBar.gameObject.SetActiveIfNeeded(true);
    }

    public void HideHpBar()
    {
        hpBar.gameObject.SetActiveIfNeeded(false);
    }

    public void DestroyCharacter()
    {
        Destroy(gameObject);
    }

    public void OnDie()
    {
        //AlkawaDebug.Log($"Gameplay: {characterConfig.characterName} die");
        characterInfo.Cell.CellType = CellType.Walkable;
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