public class DamageTakenState : CharacterState
{
    public DamageTakenState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Damage Taken";
    public override void OnEnter()
    {
        base.OnEnter();
        Character.PlayAnim(AnimationParameterNameType.OnDamageTaken, OnFinishAction);
    }

    public override void OnExit()
    {
        
    }
}