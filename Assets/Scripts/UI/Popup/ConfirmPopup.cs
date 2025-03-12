using System;
using TMPro;
using UnityEngine;

public class ConfirmPopup : PopupBase
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private UI_Button confirmButton;
    [SerializeField] private UI_Button cancelButton;

    private Action _onConfirm;
    private Action _onCancel;
    
    private void Awake()
    {
        confirmButton.button.onClick.AddListener(OnConfirmClicked);
        cancelButton.button.onClick.AddListener(OnCancelClicked);
    }
    
    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);

        if (parameters is ConfirmPopupParameters confirmPopupParameters)
        {
            titleText.text = confirmPopupParameters.Title;
            messageText.text = confirmPopupParameters.Message;
            confirmButton.label.text = confirmPopupParameters.ConfirmText;
            cancelButton.label.text = confirmPopupParameters.CancelText;
            _onConfirm = confirmPopupParameters.ConfirmAction;
            _onCancel = confirmPopupParameters.CancelAction;
        }
    }

    private void OnConfirmClicked()
    {
        Close();
        _onConfirm?.Invoke();l
    }

    private void OnCancelClicked()
    {
        _onCancel?.Invoke();
        Close();
    }
}