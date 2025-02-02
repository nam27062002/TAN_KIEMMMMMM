using UnityEngine;

public class AIMoveState : MoveState
{
    public AIMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget()
    {
        base.OnReachToTarget();
        //AlkawaDebug.Log("NT - ach to target");
    }
}