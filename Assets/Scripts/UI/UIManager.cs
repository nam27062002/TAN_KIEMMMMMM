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
    [SerializeField] private GameObject cheat;
    public SerializableDictionary<EffectType, Sprite> effectIcons;
    [SerializeField] public Sprite defaultIcon;
    public UIBase CurrentMenu { get; set; }
    public UIBase CurrentPopup { get; set; }
    
    protected override void Awake()
    {
        base.Awake();
        SetActiveAllMenus(false);
        SetActiveAllPopups(false);
#if UNITY_EDITOR
        cheat.SetActiveIfNeeded(false);
#else
        cheat.SetActiveIfNeeded(true);
#endif
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            cheat.SetActiveIfNeeded(!cheat.activeSelf);
        }
    }

    #region Main Function

    public void OpenPopup(PopupType popupType, UIBaseParameters parameters = null)
    {
        CurrentPopup?.Close();
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

    public void CloseCurrentMenu()
    {
        CurrentMenu?.Close();
    }

    #endregion
    private void OnClosePopup(object sender, EventArgs e)
    {
        greyBackground.enabled = false;
        CurrentPopup = null;
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
    Win = 7,
    Lose = 8,
    Credit = 9,
    Splash = 10,
    LoadProcess = 11,
}

public enum MenuType
{
    None = 0,
    InGame = 1,
}