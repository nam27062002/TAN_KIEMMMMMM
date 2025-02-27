public class RoiNguoi : AICharacter
{
    public override void HandleAIPlay()
    {
        AlkawaDebug.Log(ELogCategory.AI,"HandleAIPlay");
        // if (!TryMoving())
        // {
        //     GameplayManager.Instance.HandleEndTurn();
        // }
        GameplayManager.Instance.HandleEndTurn();
    }
}