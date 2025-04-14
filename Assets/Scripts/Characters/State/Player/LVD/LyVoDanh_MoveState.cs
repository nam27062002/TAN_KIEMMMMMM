public class LyVoDanh_MoveState : PlayerMoveState
{
    public LyVoDanh_MoveState(Character self) : base(self)
    {
    }
    
    protected override void OnReachToTarget(Cell from, Cell to)
    {
        base.OnReachToTarget(from, to);
        CoroutineDispatcher.Invoke(TryTriggerFlyingTempest, 0.5f);
    }

    private void TryTriggerFlyingTempest()
    {
        ((LyVoDanh)Self).TryTriggerFlyingTempest();
    }
}