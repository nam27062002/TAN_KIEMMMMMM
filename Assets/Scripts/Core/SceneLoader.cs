using UnityEngine;
using UnityEngine.SceneManagement;

public enum ESceneType
{
    Bootstrap,
    Loading,
    GameManager,
    UIManager,
    MainMenu,
    Tutorial,
    Game,
}
public static class SceneLoader
{ 
    public static AsyncOperation LoadSceneAsync(ESceneType sceneType, LoadSceneMode _loadSceneMode)
    {
        var loadSceneParameters = new LoadSceneParameters(_loadSceneMode);
        return SceneManager.LoadSceneAsync(sceneType.ToString(), loadSceneParameters);
    }

    public static AsyncOperation UnloadSceneAsync(ESceneType sceneType, UnloadSceneOptions unloadOpt = UnloadSceneOptions.None)
    {
        return SceneManager.UnloadSceneAsync(sceneType.ToString(), unloadOpt);
    }
        
    public static Scene GetSceneByName(string name)
    {
        return SceneManager.GetSceneByName(name);
    }

    public static Scene GetSceneByPath(string path)
    {
        return SceneManager.GetSceneByPath(path);
    }

    public static Scene GetSceneAt(int index)
    {
        return SceneManager.GetSceneAt(index);
    }
}