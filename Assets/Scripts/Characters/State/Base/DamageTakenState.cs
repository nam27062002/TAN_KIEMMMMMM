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
        if (stateParams is not DamageTakenParams damageTakenParams) return;
        _damageTakenParams = damageTakenParams;
        OnDamageTaken();
        PlayAnim(AnimationParameterNameType.Skill2, SetDamageTakenFinished);
    }

    private void OnDamageTaken()
    {
        Info.OnDamageTaken(_damageTakenParams);
    }
    
    protected virtual void SetDamageTakenFinished()
    {
        Character.ChangeState(ECharacterState.Idle);
        _damageTakenParams.OnSetDamageTakenFinished?.Invoke(Character);
    }
    
    public override void OnExit()
    {
    }
    
}