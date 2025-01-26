public class IdleState : CharacterState
{
    public IdleState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Idle";

    public override void OnEnter()
    {
        base.OnEnter();
        Character.PlayAnim(AnimationParameterNameType.Idle);
        SetFacing();
    }

    public override void OnExit()
    {
        
    }
}