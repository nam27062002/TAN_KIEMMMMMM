using UnityEngine;

public abstract class StateMachine
{
#if USE_DEBUG
    protected readonly bool _canShowDebug = true;
#endif
    protected IState CurrentState { get; set; }
        
    protected virtual void ChangeState(IState newState)
    {
        if (CurrentState != null && CurrentState == newState) return;
#if USE_DEBUG
        ShowDebug(newState);
#endif
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter();
    }

#if USE_DEBUG
    protected virtual void ShowDebug(IState newState) { }
#endif
}