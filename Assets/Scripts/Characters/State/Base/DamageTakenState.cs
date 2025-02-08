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
        PlayAnim(AnimationParameterNameType.Skill2, OnFinishAction);
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
        if (_damageTakenParams.ReducedMana > 0) Character.CharacterInfo.HandleMpChanged(-_damageTakenParams.ReducedMana);
    }
    
    public override void OnExit()
    {
        
    }
}