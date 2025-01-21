using System.Collections.Generic;
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
            Attributes = characterConfig.characterAttributes,
            CurrentHP = characterConfig.characterAttributes.health,
            CurrentMP = characterConfig.characterAttributes.mana,
        };
        skillConfig.SetSkillConfigs();
        SetCell(cell);
        SetCharacterPosition(cell);
        SetFacing();
        characterInfo.OnHpChanged = OnHpChanged;
        characterInfo.OnHpChanged?.Invoke();
    }

    public void ChangeState(ECharacterState newState)
    {
        StateMachine.ChangeState(newState);
    }

    public void PlayAnim(AnimationParameterNameType animationParameterNameType)
    {
        characterAnimationData.PlayAnimation(animationParameterNameType);
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
    }
#endif
}