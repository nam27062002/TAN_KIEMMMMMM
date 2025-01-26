public class DoanGiaLinh : PlayerCharacter
{
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            characterInfo.Speed = 11;
        }
        else
        {
            base.SetSpeed();
        }
    }  
}