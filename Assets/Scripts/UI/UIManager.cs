using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonoBehavior<UIManager>
{
    // new
    [SerializeField] private SerializableDictionary<PopupType, UIBase> allPopups = new();
    [SerializeField] private SerializableDictionary<PopupType, UIBase> allMenus = new();
    [SerializeField] private Image greyBackground;

    private UIBase _currentMenu;
    private UIBase _currentPopup;

    protected override void Awake()
    {
        base.Awake();
        SetActiveAllMenus(false);
        SetActiveAllPopups(false);
    }

    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        PopupBase.OnOpen += OnOpenPopup;
        PopupBase.OnClose += OnClosePopup;
    }
    
    protected override void UnRegisterEvents()
    {
        base.UnRegisterEvents();
        PopupBase.OnOpen -= OnOpenPopup;
        PopupBase.OnClose -= OnClosePopup;
    }
    
    private void OnClosePopup(object sender, EventArgs e)
    {
        greyBackground.enabled = false;
        _currentPopup = null;
    }

    private void OnOpenPopup(object sender, EventArgs e)
    {
        greyBackground.enabled = true;
    }
    
    private void SetActiveAllPopups(bool active)
    {
        foreach (var popup in allPopups.Values.ToList())
        {
            popup.gameObject.SetActiveIfNeeded(active);
        }
    }

    private void SetActiveAllMenus(bool active)
    {
        foreach (var menu in allMenus.Values.ToList())
        {
            menu.gameObject.SetActiveIfNeeded(active);
        }
    }
    
    public void OpenPopup(PopupType popupType, UIBaseParameters parameters = null)
    {
        _currentPopup?.Close();
        _currentPopup = allPopups[popupType];
        _currentPopup.Open(parameters);
    }
    
    
    // old
    [ReadOnly] public PopupBase currentPopup;
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
        // currentPopup.OpenPopup();
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
            // currentPopup.ClosePopup();
        }
    }
}


public enum PopupType
{
    None = 0,
    PauseGame = 1,
    Defeat = 2,
    WinGame = 3,
    ShowInfo = 4,
    ConfirmPopup = 5,
}

public enum MenuType
{
    None = 0,
    InGame = 1,
}