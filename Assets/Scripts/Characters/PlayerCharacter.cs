public class PlayerCharacter : Character
{
    public override Type Type => Type.Player;
    
    public override void OnSelected()
    {
        base.OnSelected();
        if (CharacterManager.MainCharacter == this)
        {
            CharacterManager.ShowMoveRange();
        }
    }
}