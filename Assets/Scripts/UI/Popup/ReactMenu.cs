using System;
using UnityEngine;
using UnityEngine.UI;

public class ReactMenu : SingletonMonoBehavior<ReactMenu>
{
    public Button cancelButton;
    public Button confirmButton;
    
    public Action OnOpen;
    
    protected override void Awake()
    {
        base.Awake();
        cancelButton.onClick.AddListener(OnCancelClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        cancelButton.onClick.RemoveListener(OnCancelClicked);
        confirmButton.onClick.RemoveListener(OnConfirmClicked);
    }

    private void OnConfirmClicked()
    {
        // GameplayManager.Instance.characterManager.OnConFirmClick();
        gameObject.SetActive(false);
    }

    private void OnCancelClicked()
    {
        if (GameplayManager.Instance.IsTutorialLevel) return;
        // GameplayManager.Instance.characterManager.OnCancelClick();
        gameObject.SetActive(false);
    }

    public void Open()
    {
        OnOpen?.Invoke();
        gameObject.SetActive(true);
    }
}