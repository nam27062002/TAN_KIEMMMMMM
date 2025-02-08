public abstract class StateMachine
{
    public IState CurrentState { get; set; }
    protected IState PreviousState { get; set; }
    protected virtual void ChangeState(IState newState, StateParams stateParams = null)
    {
        if (CurrentState != null && CurrentState == newState) return;
        ChangeStateMessage(newState);
        CurrentState?.OnExit();
        PreviousState = CurrentState;
        CurrentState = newState;
        CurrentState?.OnEnter(stateParams);
    }

    protected virtual void ChangeStateMessage(IState newState) { }
}