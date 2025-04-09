using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
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
        DOTween.KillAll();
        
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
        AlkawaDebug.Log(ELogCategory.UI, "Exit button clicked");
    }
}