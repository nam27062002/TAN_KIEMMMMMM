public class DoanGiaLinh : PlayerCharacter
{
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            CharacterInfo.Speed = 11;
        }
        else
        {
            base.SetSpeed();
        }
    }  
}