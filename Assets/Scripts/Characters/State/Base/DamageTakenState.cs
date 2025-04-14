public abstract class DamageTakenState : CharacterState
{
    public DamageTakenState(Character self) : base(self)
    {
    }

    public override string NameState { get; set; } = "Damage Taken";
    protected DamageTakenParams DamageTakenParams;
    protected abstract bool CanCounter();
    
    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        DamageTakenParams = (DamageTakenParams)stateParams;
        if (stateParams is not DamageTakenParams damageTakenParams) return;
        DamageTakenParams = damageTakenParams;
        HandleDamageTaken();
    }

    private void HandleDamageTaken()
    {
        if (DamageTakenParams.CanDodge)
        {
            Self.ShowMessage("Né");
        }
        if (CanCounter())
        {
            HandleCounter();
        }
        else
        {
            OnDamageTaken();
        }
    }

    protected abstract void HandleCounter();

    
    protected void OnDamageTaken()
    {
        Self.Info.OnDamageTaken(DamageTakenParams);
        if (DamageTakenParams.ReceiveFromCharacter != null &&
            DamageTakenParams.ReceiveFromCharacter.Type == Self.Type)
        {
            HandleBuff();
        }
        else
        {
            HandleDamage();
        }
    }

    protected virtual void HandleBuff()
    {
        PlayAnim(AnimationParameterNameType.Buff, SetDamageTakenFinished); 
    }

    protected virtual void HandleDamage()
    {
        PlayAnim(AnimationParameterNameType.OnDamageTaken, SetDamageTakenFinished); 
    }
    
    protected virtual void SetDamageTakenFinished()
    {
        DamageTakenParams.OnSetDamageTakenFinished?.Invoke(new FinishApplySkillParams()
        {
            Character = Self,
            WaitForCounter = DamageTakenParams.WaitCounter,
        });
        Self.ChangeState(ECharacterState.Idle);
        Self.Info.CheckEffectAfterReceiveDamage(DamageTakenParams);
        AlkawaDebug.Log(ELogCategory.CHARACTER, $"{Self.characterConfig.characterName} set DamageTakenFinished");
    }
    
    public override void OnExit()
    {
    }
}