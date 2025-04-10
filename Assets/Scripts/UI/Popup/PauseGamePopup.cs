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
        // Dừng các tween đang chạy
        DOTween.KillAll();

        // Dọn dẹp các đối tượng đặc biệt như bóng của CanSat (tương tự OnExitButtonClick)
        // để đảm bảo chúng được hủy đúng cách trước khi unload scene
        var canSats = FindObjectsOfType<CanSat>();
        foreach (var canSat in canSats)
        {
            // Kiểm tra null an toàn trước khi gọi DestroyCharacter
            if (canSat.dancer != null)
            {
                // Có thể gọi Destroy trực tiếp gameObject để an toàn hơn nếu DestroyCharacter có vấn đề
                // Destroy(canSat.dancer.gameObject); 
                canSat.dancer.DestroyCharacter(); // Giả định DestroyCharacter đã xử lý null check bên trong
                canSat.dancer = null;
            }

            if (canSat.assassin != null)
            {
                // Destroy(canSat.assassin.gameObject);
                canSat.assassin.DestroyCharacter();
                canSat.assassin = null;
            }
        }

        // Gọi hủy tất cả các nhân vật còn lại thông qua GameplayManager nếu cần
        // Hoặc bỏ qua nếu việc unload scene đã đủ
        // GameplayManager.Instance.DestroyAllCharacters(); // Cân nhắc xem có cần thiết không

        // --- Sửa code: Yêu cầu GameManager xử lý Replay ---
        var currentLevelType = GameplayManager.Instance.LevelConfig.levelType; 
        GameManager.Instance.RequestReplay(currentLevelType);
        // Không cần tự chuyển scene ở đây nữa
        // SceneLoader.LoadSceneAsync(ESceneType.MainMenu, LoadSceneMode.Additive);
        // SceneLoader.UnloadSceneAsync(ESceneType.Game);
        // -------------------------------------------------

        // Đóng các UI liên quan
        UIManager.Instance.CloseCurrentMenu(); // Đảm bảo menu hiện tại (nếu có) được đóng
        Close(); // Đóng popup Pause

        AlkawaDebug.Log(ELogCategory.UI, "Replay button clicked - Requesting replay via GameManager");
    }

    private void OnSaveButtonClick()
    {
        Close();
        var parameters = new 
            ConfirmPopupParameters(
                title: "Lưu trò chơi",
                message: "Bạn có muốn lưu trò chơi",
                confirmText: "Lưu",
                cancelText: "Hủy",
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