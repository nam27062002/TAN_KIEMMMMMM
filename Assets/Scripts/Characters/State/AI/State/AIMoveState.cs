public class AIMoveState : MoveState
{
    public AIMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget()
    {
        base.OnReachToTarget();
        CoroutineDispatcher.Invoke(HandlePlay, 1f);
    }

    private void HandlePlay()
    {
        ((AICharacter)Character).HandleAIPlay();
    }
}