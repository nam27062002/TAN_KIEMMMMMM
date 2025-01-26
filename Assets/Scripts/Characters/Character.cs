using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Character : MonoBehaviour
{
    public virtual Type Type => Type.None;
    public CharacterType characterType;
    public CharacterAnimationData characterAnimationData;
    public HpBar hpBar;
    public GameObject model;
    public CharacterStateMachine StateMachine { get; set; }
    public CharacterManager CharacterManager;
    protected GameplayManager GpManager => GameplayManager.Instance;
    [Title("Settings"), Space(10)] 
    public CharacterConfig characterConfig;
    public CharacterInfo characterInfo;
    public SkillConfig skillConfig;
    public event EventHandler OnEndAnimEventHandler;
    public Action OnEndAnim;
    public Roll Roll {get; set;}
    
    private void Awake()
    {
        StateMachine = new CharacterStateMachine(this);
    }
    
    public void Initialize(CharacterManager characterManager, Cell cell)
    {
        CharacterManager = characterManager;
        characterInfo = new CharacterInfo
        {
            SkillConfig = skillConfig,
            Attributes = characterConfig.characterAttributes,
            CurrentHP = characterConfig.characterAttributes.health,
            CurrentMP = characterConfig.characterAttributes.mana,
            Character = this,
        };
        skillConfig.SetSkillConfigs();
        Roll = new Roll(characterInfo.Attributes);
        SetCell(cell);
        SetCharacterPosition(cell);
        SetIdle();
        SetSpeed();
        characterInfo.OnHpChanged = OnHpChanged;
        characterInfo.OnHpChanged?.Invoke();
        ChangeState(ECharacterState.Idle);
    }

    public virtual void SetMainCharacter()
    {
        characterInfo.ResetBuffBefore();
    }

    public void CastSkill(SkillInfo skillInfo, Action onEndAnim)
    {
        OnEndAnim = onEndAnim;

        ChangeState(GetCharacterStateByIndex(skillInfo.skillIndex));
        return;

        ECharacterState GetCharacterStateByIndex(int index)
        {
            return index switch
            {
                0 => ECharacterState.Skill1,
                1 => ECharacterState.Skill2,
                2 => ECharacterState.Skill3,
                3 => ECharacterState.Skill4,
                _ => ECharacterState.None
            };
        }
    }
    
    public void ChangeState(ECharacterState newState)
    {
        StateMachine.ChangeState(newState);
    }

    public void OnEndAnimAction()
    {
        OnEndAnimEventHandler?.Invoke(this, EventArgs.Empty);
    }

    public void PlayAnim(AnimationParameterNameType animationParameterNameType, Action onEndAnim = null)
    {
        characterAnimationData.PlayAnimation(animationParameterNameType, onEndAnim);
    }

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
    
    private void OnHpChanged()
    {
        var currentHp = characterInfo.CurrentHP;
        var maxHp = characterInfo.Attributes.health;
        hpBar.SetValue(currentHp * 1f/ maxHp, $"{currentHp} / {maxHp}");
    }

    public List<SkillInfo> GetSkillInfos(SkillType skillType)
    {
        return skillConfig.SkillConfigs[skillType];
    }

    public void MoveCharacter(List<Cell> cells, Action onEndAnim = null)
    {
        characterInfo.MoveCells = cells;
        ChangeState(ECharacterState.Move);   
    }

    public void MoveCharacter(Vector3 targetPos, float duration)
    {
        // ChangeState(targetPos.x > transform.position.x ? ECharacterState.MoveRight : ECharacterState.MoveLeft);
        // transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
    }

    private void SetIdle()
    {
        ChangeState(ECharacterState.Idle);
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