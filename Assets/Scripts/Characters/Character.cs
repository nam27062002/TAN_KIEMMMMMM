using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Character : MonoBehaviour
{
    public virtual Type Type => Type.None;
    public CharacterAnimationData characterAnimationData;
    public ProcessBar hpBar;
    public GameObject model;
    public CharacterStateMachine StateMachine { get; set; }
    protected CharacterManager CharacterManager;
    [Title("Settings")] public CharacterConfig characterConfig;
    public CharacterInfo characterInfo;
    public SkillConfig skillConfig;

    private void Awake()
    {
        StateMachine = new CharacterStateMachine(this);
    }

    private void Start()
    {
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
        SetCell(cell);
        SetCharacterPosition(cell);
        SetIdle();
        characterInfo.OnHpChanged = OnHpChanged;
        characterInfo.OnHpChanged?.Invoke();
    }

    public virtual void SetMainCharacter()
    {
        characterInfo.ResetBuffBefore();
    }

    public void PlaySkillAnim(SkillInfo skillInfo, Action onEndAnim)
    {
        AnimationParameterNameType GetAnimNameByIndex(int index)
        {
            if (index == 0) return AnimationParameterNameType.Skill1;
            if (index == 1) return AnimationParameterNameType.Skill2;
            if (index == 2) return AnimationParameterNameType.Skill3;
            if (index == 3) return AnimationParameterNameType.Skill4;
            return AnimationParameterNameType.Idle;
        }
        
        PlayAnim(GetAnimNameByIndex(skillInfo.skillIndex), onEndAnim);
    }
    
    public void ChangeState(ECharacterState newState)
    {
        StateMachine.ChangeState(newState);
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

    public void MoveCharacter(List<Cell> cells)
    {
        characterInfo.Cell.HideFocus();
        characterInfo.MoveAmount += cells.Count;
        var moveSequence = DOTween.Sequence();
        float currentX = transform.position.x;
        foreach (var cell in cells)
        {
            var targetPos = cell.transform.position;
            targetPos.y += characterConfig.characterHeight / 2f;
            if (cell.transform.position.x > currentX)
            {
                PlayAnim(AnimationParameterNameType.MoveRight);
            }
            else
            {
                PlayAnim(AnimationParameterNameType.MoveLeft);
            }
            currentX = cell.transform.position.x;
            moveSequence.Append(transform.DOMove(targetPos, 0.5f).SetEase(Ease.Linear));
        }
        
        moveSequence.OnComplete(() =>
        {
            SetIdle();
            SetCell(cells[^1]);
            characterInfo.Cell.ShowFocus();
        });
    }

    public void SetIdle()
    {
        PlayAnim(AnimationParameterNameType.Idle);
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
            hpBar = GetComponentInChildren<ProcessBar>();
        }

        skillConfig.OnValidate();
    }
#endif
}