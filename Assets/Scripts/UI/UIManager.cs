using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonoBehavior<UIManager>
{
    [ReadOnly] public PopupBase currentPopup;
    public Action OnClosePopup;
    public SerializableDictionary<PopupType, PopupBase> popups;
    private bool _hasTutorialMenu;
    public Image backgroundImage;
    public void ShowPopup(PopupType popupType)
    {
        _hasTutorialMenu = MessageMenu.Instance.IsOpen;
        if (_hasTutorialMenu)
        {
            MessageMenu.Instance.HideTutorialText();
        }

        currentPopup = popups[popupType];
        currentPopup.OpenPopup();
    }
    
    public void ClosePopup()
    {
        if (_hasTutorialMenu)
        {
            MessageMenu.Instance.ShowTutorial();
        }
        if (currentPopup != null)
        {
            if (GameplayManager.Instance.IsTutorialLevel && currentPopup is ShowInfoPopup)
            {
                TutorialManager.Instance?.OnTutorialClicked(13, 0);
            }
            currentPopup.ClosePopup();
        }
    }
}


public enum PopupType
{
    None = 0,
    PauseGame = 1,
    Defeat = 2,
    ShowInfo = 3,
}