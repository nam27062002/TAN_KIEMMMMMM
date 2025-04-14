public class RoiNguoiDamageTakenState : AIDamageTakenState
{
    protected override bool CanCounter()
    {
        return false;
    }
    
    public RoiNguoiDamageTakenState(Character self) : base(self)
    {
    }
}