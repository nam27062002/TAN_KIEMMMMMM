public abstract class StateMachine
{
    protected IState CurrentState { get; set; }
    
    protected virtual void ChangeState(IState newState, StateParams stateParams = null)
    {
        if (CurrentState != null && CurrentState == newState) return;
        ChangeStateMessage(newState);
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter(stateParams);
    }

    protected virtual void ChangeStateMessage(IState newState) { }
}