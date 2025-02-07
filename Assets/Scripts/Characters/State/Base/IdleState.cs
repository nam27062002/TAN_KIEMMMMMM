public class IdleState : CharacterState
{
    public IdleState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Idle";

    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        PlayAnim(AnimationParameterNameType.Idle);
        SetFacing();
    }

    public override void OnExit()
    {
        
    }
}