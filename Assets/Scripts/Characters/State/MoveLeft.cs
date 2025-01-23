public class MoveLeft : CharacterState
{
    public MoveLeft(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "MoveLeft";
    public override void OnEnter()
    {
        base.OnEnter();
        Character.PlayAnim(AnimationParameterNameType.MoveLeft);
    }

    public override void OnExit()
    {
        
    }
}