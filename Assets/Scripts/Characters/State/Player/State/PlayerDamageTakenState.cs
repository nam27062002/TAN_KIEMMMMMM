public class PlayerDamageTakenState : DamageTakenState
{
    protected override bool CanCounter() => true;
    public PlayerDamageTakenState(Character character) : base(character)
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
        GpManager.SetCharacterReact(Character, DamageTakenParams);
    }

    private void OnCancelReact()
    {
        OnDamageTaken();
    }
}