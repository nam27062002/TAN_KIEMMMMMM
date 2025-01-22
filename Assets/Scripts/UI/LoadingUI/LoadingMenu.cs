using System.Collections;
using UnityEngine;

public class LoadingMenu : MonoBehaviour
{
    [SerializeField] private ProcessBar progressBar;
    [SerializeField] private float timeLoading;

    private void Start()
    {
        StartCoroutine(StartLoading());
    }

    private IEnumerator StartLoading()
    {
#if QUICK_CHECK
        timeLoading = 0.1f;
#endif
        float elapsedTime = 0f;
        while (elapsedTime < timeLoading)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / timeLoading);
            progressBar.SetValue(progress, $"Loading {(progress * 100):F0}%");

            yield return null; 
        }
        
        progressBar.SetValue(1f, "Loading 100%");
        Debug.Log("Loading Complete!");
        GameManager.Instance.OnLoadComplete?.Invoke();
        SceneLoader.UnloadSceneAsync(ESceneType.Loading);
    }
}