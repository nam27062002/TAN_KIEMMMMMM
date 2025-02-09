public class IdleState : CharacterState
{
    public IdleState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Idle";

    private IdleStateParams _idleStateParams;

    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        _idleStateParams = stateParams as IdleStateParams;
        PlayAnim(AnimationParameterNameType.Idle);
        SetCharacterPosition();
        SetFacing();
    }

    public override void OnExit()
    {
        
    }
}