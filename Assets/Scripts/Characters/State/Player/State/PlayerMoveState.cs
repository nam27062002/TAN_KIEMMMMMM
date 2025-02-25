public class PlayerMoveState : MoveState
{
    public PlayerMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget(Cell from, Cell to)
    {
        base.OnReachToTarget(from, to);
        Character.ShowMoveRange();
    }
}