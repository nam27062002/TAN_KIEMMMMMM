using UnityEngine.UI;

public class WinPopup : PopupBase
{
      public Button continueButton;  
      
      protected override void RegisterEvents()
      {
            base.RegisterEvents();
            continueButton.onClick.AddListener(OnWinPopup);
      }

      protected override void UnregisterEvents()
      {
            base.UnregisterEvents();    
            continueButton.onClick.RemoveListener(OnWinPopup);
      }
    
      private void OnWinPopup()
      {
            GameplayManager.Instance.DestroyGameplay();
            Close();
            AlkawaDebug.Log(ELogCategory.UI, "Replay button clicked");
      }
      
}