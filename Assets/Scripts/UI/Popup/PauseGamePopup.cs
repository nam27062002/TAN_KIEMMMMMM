using UnityEngine;
using UnityEngine.UI;

public class PauseGamePopup : PopupBase
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        replayButton.onClick.AddListener(OnReplayButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        settingsButton.onClick.AddListener(OnSettingsButtonClick);
        exitButton.onClick.AddListener(OnExitButtonClick);

    }

    protected override void UnregisterEvents()
    {
        base.UnregisterEvents();    
        replayButton.onClick.RemoveListener(OnReplayButtonClick);
        saveButton.onClick.RemoveListener(OnSaveButtonClick);
        settingsButton.onClick.RemoveListener(OnSettingsButtonClick);
        exitButton.onClick.RemoveListener(OnExitButtonClick);
    }

    private void OnReplayButtonClick()
    {
        GameplayManager.Instance.DestroyGameplay();
        Close();
        AlkawaDebug.Log(ELogCategory.UI, "Replay button clicked");
    }

    private void OnSaveButtonClick()
    {
        AlkawaDebug.Log(ELogCategory.UI, "Save button clicked");
    }

    private void OnSettingsButtonClick()
    {
        AlkawaDebug.Log(ELogCategory.UI, "Setting button clicked");
    }

    private void OnExitButtonClick()
    {
        AlkawaDebug.Log(ELogCategory.UI, "Exit button clicked");
    }
}