using UnityEngine;

public abstract class MenuBase : UIBase
{
    [SerializeField] private MenuType menuType;
    protected override string OnCloseMessage => $"Closed menu: {menuType}";
    protected override string OnOpenMessage => $"Opened menu: {menuType}";
}