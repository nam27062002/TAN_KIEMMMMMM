public class DamageTakenState : CharacterState
{
    public DamageTakenState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Damage Taken";
    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        PlayAnim(AnimationParameterNameType.OnDamageTaken, OnFinishAction);
    }

    public override void OnExit()
    {
        
    }
}