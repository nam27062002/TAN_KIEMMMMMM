public class IdleState : CharacterState
{
    public IdleState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Idle";

    public IdleStateParams IdleStateParams { get; set; }

    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        IdleStateParams = stateParams as IdleStateParams;
        SetIdle();
    }

    public override void OnExit()
    {
        base.OnExit();
        IdleStateParams = null;
    }
}