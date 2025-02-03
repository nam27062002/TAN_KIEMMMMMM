using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    protected virtual string OnCloseMessage => "Close";
    protected virtual string OnOpenMessage => "Open";
    protected virtual UIBaseParameters Parameters { get; set; }
    protected UIManager UIMng => UIManager.Instance;
    
    public virtual void Open(UIBaseParameters parameters = null)
    {
        Parameters = parameters;   
        gameObject.SetActiveIfNeeded(true);
        RegisterEvents();
        AlkawaDebug.Log(ELogCategory.UI, OnOpenMessage);
    }
    
    public virtual void Close()
    {
        gameObject.SetActiveIfNeeded(false);
        UnregisterEvents();
        AlkawaDebug.Log(ELogCategory.UI, OnCloseMessage);
    }
    
    protected virtual void RegisterEvents(){}
    protected virtual void UnregisterEvents(){}
}