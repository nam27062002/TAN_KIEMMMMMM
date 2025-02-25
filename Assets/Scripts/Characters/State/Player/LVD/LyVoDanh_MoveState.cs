public class LyVoDanh_MoveState : PlayerMoveState
{
    public LyVoDanh_MoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget(Cell from, Cell to)
    {
        base.OnReachToTarget(from, to);
        CoroutineDispatcher.Invoke(TryTriggerFlyingTempest, 0.5f);
    }

    private void TryTriggerFlyingTempest()
    {
        ((LyVoDanh)Character).TryTriggerFlyingTempest();
    }
}