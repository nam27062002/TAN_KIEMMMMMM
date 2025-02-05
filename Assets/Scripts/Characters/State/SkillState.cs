using UnityEngine;

public class SkillState : CharacterState
{
    public SkillState(Character character) : base(character) { }

    public override string NameState { get; set; } = "Skill"; 
    
    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        HandleCastSkill();
    }

    private void HandleCastSkill()
    {
        var animName = GetAnimByIndex(Character.characterInfo.SkillInfo.skillIndex);
        Character.PlayAnim(animName, OnFinishAction);
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"Handle cast skill: {animName}");
    }
    
    private static AnimationParameterNameType GetAnimByIndex(int index)
    {
        return index switch
        {
            0 => AnimationParameterNameType.Skill1,
            1 => AnimationParameterNameType.Skill2,
            2 => AnimationParameterNameType.Skill3,
            3 => AnimationParameterNameType.Skill4,
            _ => AnimationParameterNameType.None
        };
    }
}