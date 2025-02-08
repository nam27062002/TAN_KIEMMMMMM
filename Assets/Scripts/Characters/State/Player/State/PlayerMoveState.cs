public class PlayerMoveState : MoveState
{
    public PlayerMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget(Cell cell)
    {
        base.OnReachToTarget(cell);
        Character.ShowMoveRange();
    }
}