public class PlayerMoveState : MoveState
{
    public PlayerMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget()
    {
        base.OnReachToTarget();
        Character.ShowMoveRange();
    }
}