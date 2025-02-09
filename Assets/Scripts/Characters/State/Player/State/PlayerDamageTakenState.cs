public class PlayerDamageTakenState : DamageTakenState
{
    public PlayerDamageTakenState(Character character) : base(character)
    {
    }
    
    protected override bool CanCounter => true;
}