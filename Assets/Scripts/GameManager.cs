using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Action = System.Action;
using Application = UnityEngine.Application;

public class GameManager : SingletonMonoBehavior<GameManager>
{
    private ESceneType _nextScene;
    public int saveIndex = -1;

    // --- Thêm code cho Replay ---
    public LevelType? LevelToReplay { get; private set; } = null;
    public bool IsReplaying { get; private set; } = false;
    // ---------------------------

    #region Action

    public Action OnLoadComplete;
    
    // gameplay
    public Action OnMainCharacterChanged;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        OnLoadComplete += HandleLoadComplete;
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        DOTween.SetTweensCapacity(500, 125);
        Application.runInBackground = true;
    }

    private void Start()
    {
        // --- Sửa code ---
        // Không load UI Manager và MainMenu ngay lập tức nếu đang replay
        if (!IsReplaying)
        {
            SceneLoader.LoadSceneAsync(ESceneType.UIManager, LoadSceneMode.Additive);
            Loading(ESceneType.MainMenu);
        }
        // Nếu IsReplaying là true, MainMenu sẽ tự gọi StartRequestedGame()
        // ----------------
    }

    public void StartGameAtSaveSlot(int saveIndex)
    {
        // Reset trạng thái replay khi load game từ save
        IsReplaying = false;
        LevelToReplay = null;
        // --- Kết thúc sửa ---

        this.saveIndex = saveIndex;
        Loading(ESceneType.Game);
        SceneLoader.UnloadSceneAsync(ESceneType.MainMenu);
    }

    public void StartNewGame()
    {
        // Reset trạng thái replay khi bắt đầu game mới
        IsReplaying = false;
        LevelToReplay = null;
        // --- Kết thúc sửa ---

        saveIndex = -1;
        Loading(ESceneType.Game);
        SceneLoader.UnloadSceneAsync(ESceneType.MainMenu);
    }

    // --- Thêm phương thức mới ---
    /// <summary>
    /// Được gọi bởi MainMenu để bắt đầu game, xử lý cả trường hợp replay và new game.
    /// </summary>
    public void StartRequestedGame()
    {
        if (IsReplaying && LevelToReplay.HasValue)
        {
            AlkawaDebug.Log(ELogCategory.ENGINE, $"Replaying level: {LevelToReplay.Value}");
            saveIndex = -1; // Đảm bảo replay là chơi mới, không load save
            Loading(ESceneType.Game);
            SceneLoader.UnloadSceneAsync(ESceneType.MainMenu);
            // Không reset flag ở đây, để GameplayManager đọc và reset
        }
        else
        {
            AlkawaDebug.Log(ELogCategory.ENGINE, "Starting new game from Main Menu.");
            StartNewGame(); // Nếu không phải replay, bắt đầu game mới
        }
    }

    /// <summary>
    /// Được gọi bởi PauseGamePopup khi nhấn Replay.
    /// </summary>
    public void RequestReplay(LevelType levelType)
    {
        AlkawaDebug.Log(ELogCategory.ENGINE, $"Requesting replay for level: {levelType}");
        IsReplaying = true;
        LevelToReplay = levelType;
        // Load MainMenu, MainMenu.Start() sẽ kiểm tra IsReplaying
        LoadMainMenu();
    }
    // --------------------------

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnLoadComplete -= HandleLoadComplete;
    }

    private void Loading(ESceneType eSceneType)
    {
        SceneLoader.LoadSceneAsync(ESceneType.Loading, LoadSceneMode.Additive);
        _nextScene = eSceneType;
        AlkawaDebug.Log(ELogCategory.ENGINE, "Loading scene " + eSceneType);
    }

    private void HandleLoadComplete()
    {
        SceneLoader.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
        SceneLoader.UnloadSceneAsync(ESceneType.Loading);
        AlkawaDebug.Log(ELogCategory.ENGINE, "Loaded scene " + _nextScene);
    }

    public void LoadMainMenu()
    {
        // Hủy scene Game nếu đang tồn tại
        SceneLoader.UnloadSceneAsync(ESceneType.Game);
        // Load scene Loading trước, sau đó Loading sẽ load MainMenu
        Loading(ESceneType.MainMenu);
    }

    // Thêm phương thức mới
    public void ResetReplayState()
    {
        AlkawaDebug.Log(ELogCategory.ENGINE, "Resetting replay state");
        IsReplaying = false;
        LevelToReplay = null;
    }
}