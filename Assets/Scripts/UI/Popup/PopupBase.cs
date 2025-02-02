using UnityEngine;

// public class PopupBase : MonoBehaviour
// {
//     private void Awake()
//     {
//         gameObject.SetActive(false);
//     }
//         
//     public virtual void OpenPopup()
//     {
//         gameObject.SetActive(true);
//         UIManager.Instance.currentPopup = this;
//         UIManager.Instance.backgroundImage.enabled = true;
//         Time.timeScale = 0;
//     }
//
//     public virtual void ClosePopup()
//     {
//         gameObject.SetActive(false);
//         UIManager.Instance.currentPopup = null;
//         UIManager.Instance.backgroundImage.enabled = false;
//         Time.timeScale = 1;
//     }
// }