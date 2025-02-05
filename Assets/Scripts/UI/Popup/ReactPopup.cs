using UnityEngine.UI;

public class ReactPopup : PopupBase
{
    public Button cancelButton;
    public Button confirmButton;
    protected override bool ShowGreyBackground => false;
    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        cancelButton.onClick.AddListener(OnCancelClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }
    
    protected override void UnregisterEvents()
    {
        base.UnregisterEvents();
        cancelButton.onClick.RemoveListener(OnCancelClicked);
        confirmButton.onClick.RemoveListener(OnConfirmClicked);
    }

    private void OnConfirmClicked()
    {
        GameplayManager.Instance.OnConFirmClick();
        Close();
    }

    private void OnCancelClicked()
    {
        if (GameplayManager.Instance.IsTutorialLevel) return;
        GameplayManager.Instance.OnCancelClick();
        Close();
    }
}