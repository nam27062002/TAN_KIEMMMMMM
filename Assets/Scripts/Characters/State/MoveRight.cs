public class MoveRight : CharacterState
{
    public MoveRight(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "MoveRight";
    public override void OnEnter()
    {
        base.OnEnter();
        Character.PlayAnim(AnimationParameterNameType.MoveRight);
    }

    public override void OnExit()
    {
        
    }
}