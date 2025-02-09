using System;
using UnityEngine.UI;

public class ReactPopup : PopupBase
{
    public Button cancelButton;
    public Button confirmButton;
    public Action OnConfirm;
    public Action OnCancel;
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

    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);
        if (parameters is not ReactPopupParameters reactPopupParameters) return;
        OnConfirm += reactPopupParameters.OnConfirm;
        OnCancel += reactPopupParameters.OnCancel;
    }
    
    private void OnConfirmClicked()
    {
        if (GameplayManager.Instance.IsTutorialLevel) return;
        OnConfirm?.Invoke();
        Close();
    }

    private void OnCancelClicked()
    {
        if (GameplayManager.Instance.IsTutorialLevel) return;
        OnCancel?.Invoke();
        Close();
    }
}