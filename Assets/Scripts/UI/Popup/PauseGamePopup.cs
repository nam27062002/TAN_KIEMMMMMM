using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        DOTween.KillAll();
        var canSats = FindObjectsOfType<CanSat>();
        foreach (var canSat in canSats)
        {
            if (canSat.dancer != null)
            {
                canSat.dancer.DestroyCharacter(); 
                canSat.dancer = null;
            }

            if (canSat.assassin != null)
            {
                canSat.assassin.DestroyCharacter();
                canSat.assassin = null;
            }
        }
        var currentLevelType = GameplayManager.Instance.LevelConfig.levelType; 
        GameManager.Instance.RequestReplay(currentLevelType);
        UIManager.Instance.CloseCurrentMenu(); 
        Close(); 
        AlkawaDebug.Log(ELogCategory.UI, "Replay button clicked - Requesting replay via GameManager");
    }

    private void OnSaveButtonClick()
    {
        Close();
        var parameters = new 
            ConfirmPopupParameters(
                title: "Save game progress?",
                message: "Do you want to save this game progress?",
                confirmText: "Save",
                cancelText: "Cancel",
                confirmAction: () => GameplayManager.Instance.OnSave()
                );
        UIManager.Instance.OpenPopup(PopupType.ConfirmPopup, parameters);
        AlkawaDebug.Log(ELogCategory.UI, "Save button clicked");
    }

    private void OnSettingsButtonClick()
    {
        AlkawaDebug.Log(ELogCategory.UI, "Setting button clicked");
    }

    private void OnExitButtonClick()
    {
        DOTween.KillAll();
        
        // Reset lại trạng thái replay
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetReplayState();
        }
        
        // Gọi DestroyAllCharacters để đảm bảo các bóng của CanSat được hủy
        var canSats = FindObjectsOfType<CanSat>();
        foreach (var canSat in canSats)
        {
            if (canSat.dancer != null)
            {
                canSat.dancer.DestroyCharacter();
                canSat.dancer = null;
            }

            if (canSat.assassin != null)
            {
                canSat.assassin.DestroyCharacter();
                canSat.assassin = null;
            }
        }
        
        // Gọi hủy tất cả các nhân vật
        GameplayManager.Instance.DestroyAllCharacters();
        
        SceneLoader.LoadSceneAsync(ESceneType.MainMenu, LoadSceneMode.Additive);
        SceneLoader.UnloadSceneAsync(ESceneType.Game);
        UIManager.Instance.CloseCurrentMenu();
        Close();
        AlkawaDebug.Log(ELogCategory.UI, "Exit button clicked - Returning to main menu (not replaying)");
    }
}