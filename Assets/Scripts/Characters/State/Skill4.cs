public class Skill4
    : CharacterState
{
    public Skill4
        (Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Skill 3";
    
    public override void OnEnter()
    {
        base.OnEnter();
        Character.PlayAnim(AnimationParameterNameType.Skill4, OnFinishAction);
    }

    public override void OnExit()
    {
        
    }
}