using DG.Tweening;
using Unity.Android.Gradle.Manifest;
using UnityEngine.SceneManagement;
using Action = System.Action;
using Application = UnityEngine.Application;

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
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        DOTween.SetTweensCapacity(500, 125);
        Application.runInBackground = true;
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
        AlkawaDebug.Log(ELogCategory.ENGINE, "Loading scene " + eSceneType);
    }

    private void HandleLoadComplete()
    {
        SceneLoader.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
        SceneLoader.UnloadSceneAsync(ESceneType.Loading);
        AlkawaDebug.Log(ELogCategory.ENGINE, "Loaded scene " + _nextScene);
    }
}