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
        // Screen.SetResolution(640, 360, FullScreenMode.Windowed);
        Application.runInBackground = true;
    }

    private void Start()
    {
        SceneLoader.LoadSceneAsync(ESceneType.UIManager, LoadSceneMode.Additive);
        Loading(ESceneType.MainMenu);
    }

    public void StartGameAtSaveSlot(int saveIndex)
    {
        this.saveIndex = saveIndex;
        Loading(ESceneType.Game);
        SceneLoader.UnloadSceneAsync(ESceneType.MainMenu);
    }

    public void StartNewGame()
    {
        saveIndex = -1;
        Loading(ESceneType.Game);
        SceneLoader.UnloadSceneAsync(ESceneType.MainMenu);
    }

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
}