public class LVDMoveState : PlayerMoveState
{
    public LVDMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget()
    {
        base.OnReachToTarget();
        CoroutineDispatcher.Invoke(TryTriggerFlyingTempest, 0.5f);
    }

    private void TryTriggerFlyingTempest()
    {
        ((LyVoDanh)Character).TryTriggerFlyingTempest();
    }
}