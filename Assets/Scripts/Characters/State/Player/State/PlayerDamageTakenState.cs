public class PlayerDamageTakenState : DamageTakenState
{
    protected override bool CanCounter()
    {
        return DamageTakenParams.CanCounter;
    }

    public PlayerDamageTakenState(Character character) : base(character)
    {
    }
    
    protected override void HandleCounter()
    {
        AlkawaDebug.Log(ELogCategory.SKILL, $"{Character.characterConfig.characterName} is under attacked by {DamageTakenParams.ReceiveFromCharacter.characterConfig.characterName}");
        
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