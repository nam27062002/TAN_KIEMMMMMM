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
    protected CharacterManager CharacterManager;
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
        ChangeState(ECharacterState.Idle);
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
    }

    public virtual void SetMainCharacter()
    {
        characterInfo.ResetBuffBefore();
    }

    public void CastSkill(SkillInfo skillInfo, Action onEndAnim)
    {
        OnEndAnim = onEndAnim;
        ECharacterState GetCharacterStateByIndex(int index)
        {
            if (index == 0) return ECharacterState.Skill1;
            if (index == 1) return ECharacterState.Skill2;
            if (index == 2) return ECharacterState.Skill3;
            if (index == 3) return ECharacterState.Skill4;
            return ECharacterState.None;
        }
        
        ChangeState(GetCharacterStateByIndex(skillInfo.skillIndex));
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

    private void SetCell(Cell cell)
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

    private void SetFacing()
    {
        var facing = CharacterManager.GetFacingType(this);
        SetFacing(facing);
    }

    private void ReleaseFacing()
    {
        model.transform.localScale = Vector3.one;
    }

    private void SetFacing(FacingType facing)
    {
        model.transform.localScale = facing == FacingType.Right ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
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
        ReleaseFacing();
        characterInfo.Cell.HideFocus();
        characterInfo.MoveAmount += cells.Count;
        var moveSequence = DOTween.Sequence();
        float currentX = transform.position.x;
        foreach (var cell in cells)
        {
            var targetPos = cell.transform.position;
            targetPos.y += characterConfig.characterHeight / 2f;
            ChangeState(cell.transform.position.x > currentX ? ECharacterState.MoveRight : ECharacterState.MoveLeft);
            currentX = cell.transform.position.x;
            moveSequence.Append(transform.DOMove(targetPos, 0.5f).SetEase(Ease.Linear));
        }
        
        moveSequence.OnComplete(() =>
        {
            SetIdle();
            SetCell(cells[^1]);
            characterInfo.Cell.ShowFocus();
            onEndAnim?.Invoke();
        });
    }

    public void MoveCharacter(Vector3 targetPos, float duration)
    {
        ChangeState(targetPos.x > transform.position.x ? ECharacterState.MoveRight : ECharacterState.MoveLeft);
        transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
    }

    private void SetIdle()
    {
        ChangeState(ECharacterState.Idle);
        SetFacing();
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