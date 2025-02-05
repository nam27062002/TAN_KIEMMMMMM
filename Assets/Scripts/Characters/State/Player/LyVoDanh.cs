public class LyVoDanh : PlayerCharacter
{
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            characterInfo.Speed = 10;
        }
        else
        {
            base.SetSpeed();
        }
    }       
}