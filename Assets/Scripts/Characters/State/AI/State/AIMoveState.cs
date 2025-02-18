public class AIMoveState : MoveState
{
    public AIMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget(Cell cell)
    {
        base.OnReachToTarget(cell);
        CoroutineDispatcher.Invoke(HandlePlay, 1f);
    }

    private void HandlePlay()
    {
        ((AICharacter)Character).HandleAIPlay();
    }
}

public enum DirectionType
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4,
    UpRight = 5,
    UpLeft = 6,
    DownLeft = 7,
    DownRight = 8,
    All = 9
}