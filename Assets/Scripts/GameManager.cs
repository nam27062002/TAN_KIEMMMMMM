using System;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonoBehavior<GameManager>
{
    private ESceneType _nextScene;
    
    #region Action

    public Action OnLoadComplete;

    #endregion
    
    
    protected override void Awake()
    {
        base.Awake();
        OnLoadComplete += HandleLoadComplete;
    }

    private void Start()
    {
        SceneLoader.LoadSceneAsync(ESceneType.UIManager, LoadSceneMode.Additive);
        Loading(ESceneType.MainMenu);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnLoadComplete -= HandleLoadComplete;
    }


    public void Loading(ESceneType eSceneType)
    {
        SceneLoader.LoadSceneAsync(ESceneType.Loading, LoadSceneMode.Additive);
        _nextScene = eSceneType;
    }

    private void HandleLoadComplete()
    {
        SceneLoader.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
    }
}