using UnityEngine;
using UnityEngine.UI;

public class LosePopup : PopupBase
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button exitButton;
    
    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        replayButton.onClick.AddListener(OnReplayButtonClick);
        exitButton.onClick.AddListener(OnExitButtonClick);

    }

    protected override void UnregisterEvents()
    {
        base.UnregisterEvents();    
        replayButton.onClick.RemoveListener(OnReplayButtonClick);
        exitButton.onClick.RemoveListener(OnExitButtonClick);
    }
    
    private void OnReplayButtonClick()
    {
        GameplayManager.Instance.DestroyGameplay();
        Close();
        AlkawaDebug.Log(ELogCategory.UI, "Replay button clicked");
    }
    
    private void OnExitButtonClick()
    {
        AlkawaDebug.Log(ELogCategory.UI, "Exit button clicked");
    }
}