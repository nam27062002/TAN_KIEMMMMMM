public class PlayerDamageTakenState : DamageTakenState
{
    protected override bool CanCounter()
    {
        return DamageTakenParams.CanCounter;
    }

    public PlayerDamageTakenState(Character self) : base(self)
    {
    }
    
    protected override void HandleCounter()
    {
        UIManager.Instance.OpenPopup(PopupType.React, new ReactPopupParameters()
        {
            OnConfirm = OnConFirmReact,
            OnCancel = OnCancelReact,
        });
    }
    
    private void OnConFirmReact()
    {
        GpManager.SetCharacterReact(Self, DamageTakenParams);
    }

    private void OnCancelReact()
    {
        OnDamageTaken();
    }
}