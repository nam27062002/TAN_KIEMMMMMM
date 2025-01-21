public class DamageTaken : CharacterState
{
    public DamageTaken(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Damage Taken";
    public override void OnEnter()
    {
        
    }

    public override void OnExit()
    {
        
    }
}