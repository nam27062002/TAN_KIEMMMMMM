public class DamageTakenState : CharacterState
{
    public DamageTakenState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Damage Taken";
    private DamageTakenParams _damageTakenParams;
    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        _damageTakenParams = (DamageTakenParams)stateParams;
        OnDamageTaken();
        PlayAnim(AnimationParameterNameType.OnDamageTaken, OnFinishAction);
    }

    private void OnDamageTaken()
    {
        
    }
    
    public override void OnExit()
    {
        
    }
}