using System;
using UnityEngine.UI;

public class ReactMenu : SingletonMonoBehavior<ReactMenu>
{
    public Button cancelButton;
    public Button confirmButton;
    
    public Action OnConFirmClick;
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
        OnConFirmClick?.Invoke();
        // CharacterManager.Instance.OnReact();
        gameObject.SetActive(false);
    }

    private void OnCancelClicked()
    {
        if (GameplayManager.Instance.IsTutorialLevel) return;
        // CharacterManager.Instance.CallOnEndAnimFeedback();
        gameObject.SetActive(false);
    }

    public void Open()
    {
        OnOpen?.Invoke();
        gameObject.SetActive(true);
    }
}