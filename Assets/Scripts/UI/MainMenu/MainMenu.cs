using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
     [SerializeField] private Button startGameButton;
     [SerializeField] private Button loadGameButton;
     [SerializeField] private Button settingGameButton;
     [SerializeField] private Button exitGameButton;

     private void Start()
     {
          startGameButton.onClick.AddListener(OnStartGameClicked);
          loadGameButton.onClick.AddListener(OnLoadGameClicked);
          settingGameButton.onClick.AddListener(OnSettingGameClicked);
          exitGameButton.onClick.AddListener(OnExitGameClicked);
          
#if UNITY_EDITOR
          startGameButton.onClick.Invoke();
#endif
     }

     private void OnDestroy()
     {
          startGameButton.onClick.RemoveListener(OnStartGameClicked);
          loadGameButton.onClick.RemoveListener(OnLoadGameClicked);
          settingGameButton.onClick.RemoveListener(OnSettingGameClicked);
          exitGameButton.onClick.RemoveListener(OnExitGameClicked);
     }

     private void OnStartGameClicked()
     {
          Debug.Log("StartGameClicked");
          SceneLoader.LoadSceneAsync(ESceneType.Game, LoadSceneMode.Additive);
          SceneLoader.UnloadSceneAsync(ESceneType.MainMenu);
     }

     private void OnLoadGameClicked()
     {
          
     }

     private void OnSettingGameClicked()
     {
          
     }

     private void OnExitGameClicked()
     {
          
     }
}