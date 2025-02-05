using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonoBehavior<UIManager>
{
    // new
    [SerializeField] private SerializableDictionary<PopupType, UIBase> allPopups = new();
    [SerializeField] private SerializableDictionary<MenuType, UIBase> allMenus = new();
    [SerializeField] private Image greyBackground;

    public UIBase CurrentMenu { get; set; }
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

    #region Main Function

    public void OpenPopup(PopupType popupType, UIBaseParameters parameters = null)
    {
        _currentPopup?.Close();
        _currentPopup = allPopups[popupType];
        _currentPopup.Open(parameters);
    }

    public void TryClosePopup(PopupType popupType)
    {
        if (_currentPopup == allPopups[popupType])
        {
            _currentPopup.Close();
        }
    }
    
    public void OpenMenu(MenuType menuType)
    {
        CurrentMenu?.Close();
        CurrentMenu = allMenus[menuType];
        CurrentMenu.Open();
    }

    #endregion
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
}

public enum PopupType
{
    None = 0,
    PauseGame = 1,
    Conversation = 2,
    Message = 3,
    ShowInfo = 4,
    ConfirmPopup = 5,
}

public enum MenuType
{
    None = 0,
    InGame = 1,
}