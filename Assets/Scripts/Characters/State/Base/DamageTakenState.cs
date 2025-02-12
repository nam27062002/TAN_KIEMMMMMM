public class DamageTakenState : CharacterState
{
    public DamageTakenState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Damage Taken";
    private DamageTakenParams _damageTakenParams;

    protected virtual bool CanCounter => false;

    public override void OnEnter(StateParams stateParams = null)
    {
        base.OnEnter(stateParams);
        _damageTakenParams = (DamageTakenParams)stateParams;
        if (stateParams is not DamageTakenParams damageTakenParams) return;
        _damageTakenParams = damageTakenParams;
        HandleDamageTaken();
    }

    private void HandleDamageTaken()
    {
        if (_damageTakenParams.CanDodge)
        {
            Character.ShowMessage("Né");
        }
        if (CanCounter && _damageTakenParams.CanCounter)
        {
            HandleCounter();
        }
        else
        {
            OnDamageTaken();
        }
    }

    private void HandleCounter()
    {
        UIManager.Instance.OpenPopup(PopupType.React, new ReactPopupParameters()
        {
            OnConfirm = OnConFirmReact,
            OnCancel = OnCancelReact,
        });
    }

    private void OnConFirmReact()
    {
        GpManager.SetCharacterReact(Character, _damageTakenParams);
    }

    private void OnCancelReact()
    {
        OnDamageTaken();
    }
    
    private void OnDamageTaken()
    {
        Character.CharacterInfo.OnDamageTaken(_damageTakenParams);
        PlayAnim(AnimationParameterNameType.OnDamageTaken, SetDamageTakenFinished); 
    }
    
    protected virtual void SetDamageTakenFinished()
    {
        _damageTakenParams.OnSetDamageTakenFinished?.Invoke(new FinishApplySkillParams()
        {
            Character = Character,
            WaitForCounter = _damageTakenParams.WaitCounter,
        });
        Character.ChangeState(ECharacterState.Idle);
        AlkawaDebug.Log(ELogCategory.CHARACTER, $"{Character.characterConfig.characterName} set DamageTakenFinished");
    }
    
    public override void OnExit()
    {
    }
}