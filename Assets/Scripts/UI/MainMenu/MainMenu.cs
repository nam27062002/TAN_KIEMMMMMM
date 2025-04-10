using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
     [Title("Buttons")]
     [SerializeField] private Button startGameButton;
     [SerializeField] private Button loadGameButton;
     [SerializeField] private Button settingGameButton;
     [SerializeField] private Button exitGameButton;

     [Title("Scriptable Object")]
     [SerializeField] private ConfirmPopupSO confirmPopup;
     private void Start()
     {
          if (GameManager.Instance != null && GameManager.Instance.IsReplaying)
          {
               AlkawaDebug.Log(ELogCategory.UI, "MainMenu detected Replay state. Starting requested game...");
               GameManager.Instance.StartRequestedGame();
          }
          else
          {
               AlkawaDebug.Log(ELogCategory.UI, "MainMenu started normally.");
               startGameButton.onClick.AddListener(OnStartGameClicked);
               loadGameButton.onClick.AddListener(OnLoadGameClicked);
               settingGameButton.onClick.AddListener(OnSettingGameClicked);
               exitGameButton.onClick.AddListener(OnExitGameClicked);
          }
          
// #if UNITY_EDITOR
//           startGameButton.onClick.Invoke();
// #endif
     }

     private void OnDestroy()
     {
          if (startGameButton != null && startGameButton.onClick != null)
               startGameButton.onClick.RemoveListener(OnStartGameClicked);
          if (loadGameButton != null && loadGameButton.onClick != null)
               loadGameButton.onClick.RemoveListener(OnLoadGameClicked);
          if (settingGameButton != null && settingGameButton.onClick != null)
               settingGameButton.onClick.RemoveListener(OnSettingGameClicked);
          if (exitGameButton != null && exitGameButton.onClick != null)
               exitGameButton.onClick.RemoveListener(OnExitGameClicked);
     }

     private void OnStartGameClicked()
     {
          GameManager.Instance.StartRequestedGame();
     }

     private void OnLoadGameClicked()
     {
          UIManager.Instance.OpenPopup(PopupType.LoadProcess);
     }

     private void OnSettingGameClicked()
     {
         
     }

     private void OnExitGameClicked()
     {
          var parameters = new ConfirmPopupParameters(
               confirmPopup.title, 
               confirmPopup.message, 
               confirmPopup.confirmText,
               confirmPopup.cancelText,
               OnQuitGameClicked);
          UIManager.Instance.OpenPopup(PopupType.ConfirmPopup, parameters);
     }

     private void OnQuitGameClicked()
     {
#if UNITY_EDITOR
          UnityEditor.EditorApplication.isPlaying = false;
#else
          Application.Quit();
#endif
     }
}