using UnityEngine;

public abstract class StateMachine
{
    public IState CurrentState { get; set; }
        
    public void ChangeState(IState newState)
    {
        Debug.Log($"[Gameplay] - change state from [{CurrentState?.NameState}] to [{newState?.NameState}]");
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter();
    }
}