public class LyVoDanh_MoveState : PlayerMoveState
{
    public LyVoDanh_MoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget(Cell cell)
    {
        base.OnReachToTarget(cell);
        CoroutineDispatcher.Invoke(TryTriggerFlyingTempest, 0.5f);
    }

    private void TryTriggerFlyingTempest()
    {
        ((LyVoDanh)Character).TryTriggerFlyingTempest();
    }
}