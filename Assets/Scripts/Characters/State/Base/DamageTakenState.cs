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
        ChangeHp();
        ChangeMana();
    }

    private void ChangeHp()
    {
        if (_damageTakenParams.Damage > 0)
        {
            Character.CharacterInfo.HandleHpChanged(-_damageTakenParams.Damage);
        }
    }

    private void ChangeMana()
    {
        if (_damageTakenParams.ReducedMana > 0)
            Character.CharacterInfo.HandleMpChanged(-_damageTakenParams.ReducedMana);
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