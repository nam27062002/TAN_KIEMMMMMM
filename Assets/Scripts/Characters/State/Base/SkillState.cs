using System;

public class SkillState : CharacterState
{
    public SkillState(Character character) : base(character) { }

    public override string NameState { get; set; } = "Skill";

    private SkillStateParams _skillStateParams;
    
    public override void OnEnter(StateParams stateParams = null)
    {
        _skillStateParams = (SkillStateParams)stateParams;
        base.OnEnter(stateParams);
        HandleCastSkill();
    }

    private void HandleCastSkill()
    {
        var animName = GetAnimByIndex(_skillStateParams.skillInfo.skillIndex);
        switch (_skillStateParams.skillInfo.skillIndex)
        {
            case SkillIndex.ActiveSkill1:
                HandleActiveSkill1();
                break;
            case SkillIndex.ActiveSkill2:
                HandleActiveSkill2();
                break;
            case SkillIndex.ActiveSkill3:
                HandleActiveSkill3();
                break;
            case SkillIndex.ActiveSkill4:
                HandleActiveSkill4();
                break;
            case SkillIndex.PassiveSkill1:
                HandlePassiveSkill1();
                break;
            case SkillIndex.PassiveSkill2:
                HandlePassiveSkill2();
                break;
            case SkillIndex.PassiveSkill3:
                HandlePassiveSkill3();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        PlayAnim(animName, OnFinishAction);
        AlkawaDebug.Log(ELogCategory.CHARACTER, $"{Character.characterConfig.characterName} cast skill: {_skillStateParams.skillInfo.name}");
    }
    
    protected virtual void HandleActiveSkill1()
    {
    }
    
    protected virtual void HandleActiveSkill2()
    {
        
    }
    
    protected virtual void HandleActiveSkill3()
    {
        
    }
    
    protected virtual void HandleActiveSkill4()
    {
        
    }

    protected virtual void HandlePassiveSkill1()
    {
        
    }

    protected virtual void HandlePassiveSkill2()
    {
        
    }

    protected virtual void HandlePassiveSkill3()
    {
        
    }
    
    private static AnimationParameterNameType GetAnimByIndex(SkillIndex index)
    {
        return index switch
        {
            SkillIndex.ActiveSkill1 => AnimationParameterNameType.Skill1,
            SkillIndex.ActiveSkill2  => AnimationParameterNameType.Skill2,
            SkillIndex.ActiveSkill3  => AnimationParameterNameType.Skill3,
            SkillIndex.ActiveSkill4  => AnimationParameterNameType.Skill4,
            SkillIndex.PassiveSkill1  => AnimationParameterNameType.Skill1,
            SkillIndex.PassiveSkill2  => AnimationParameterNameType.Skill1,
            SkillIndex.PassiveSkill3  => AnimationParameterNameType.Skill1,
            _ => AnimationParameterNameType.None
        };
    }
}