public abstract class StateMachine
{
    protected IState CurrentState { get; set; }
    
    protected virtual void ChangeState(IState newState)
    {
        if (CurrentState != null && CurrentState == newState) return;
        ChangeStateMessage(newState);
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter();
    }

    protected virtual void ChangeStateMessage(IState newState) { }
}