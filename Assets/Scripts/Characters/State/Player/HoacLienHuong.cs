public class HoacLienHuong : PlayerCharacter
{
    protected override void SetSpeed()
    {
        base.SetSpeed();
        CharacterInfo.Speed = 100;
    }
}