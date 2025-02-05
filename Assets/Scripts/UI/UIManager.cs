using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonoBehavior<UIManager>
{
    [SerializeField] private SerializableDictionary<PopupType, UIBase> allPopups = new();
    [SerializeField] private SerializableDictionary<MenuType, UIBase> allMenus = new();
    [SerializeField] private Image greyBackground;
    
    public UIBase CurrentMenu { get; set; }
    public UIBase CurrentPopup { get; set; }
    
    private Stack<UIBase> popupStack = new Stack<UIBase>();

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

    public void OpenPopup(PopupType popupType, UIBaseParameters parameters = null)
    {
        if (CurrentPopup != null)
        {
            popupStack.Push(CurrentPopup);
            CurrentPopup.gameObject.SetActive(false);
        }
        
        CurrentPopup = allPopups[popupType];
        CurrentPopup.Open(parameters);
    }

    public void TryClosePopup(PopupType popupType)
    {
        if (CurrentPopup == allPopups[popupType])
        {
            CurrentPopup.Close();
        }
    }
    
    public void OpenMenu(MenuType menuType)
    {
        CurrentMenu?.Close();
        CurrentMenu = allMenus[menuType];
        CurrentMenu.Open();
    }

    private void OnClosePopup(object sender, EventArgs e)
    {
        CurrentPopup = null;
        greyBackground.enabled = false;
        if (popupStack.Count > 0)
        {
            UIBase previousPopup = popupStack.Pop();
            CurrentPopup = previousPopup;
            CurrentPopup.gameObject.SetActive(true);
            greyBackground.enabled = true;
        }
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
    React = 6,
}

public enum MenuType
{
    None = 0,
    InGame = 1,
}
