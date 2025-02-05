public class ReactState : CharacterState
{
    public ReactState(Character character) : base(character)
    {
    }
    public override string NameState { get; set; } = "React";
    
    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        Character.PlayAnim(AnimationParameterNameType.Idle);
        SetFacing();
    }

    public override void OnExit()
    {
        
    }
}