public class PlayerMoveState : MoveState
{
    public PlayerMoveState(Character self) : base(self)
    {
    }
    
    protected override void OnReachToTarget(Cell from, Cell to)
    {
        base.OnReachToTarget(from, to);
        Self.ShowMoveRange();
    }
}