using UnityEngine;

public class AIMoveState : MoveState
{
    public AIMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget()
    {
        base.OnReachToTarget();
        ((AICharacter)Character).HandleAIPlay();
    }
}