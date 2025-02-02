using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    protected virtual string OnCloseMessage => "Close";
    protected virtual string OnOpenMessage => "Open";
    protected virtual UIBaseParameters Parameters { get; set; }
    public virtual void Close()
    {
        gameObject.SetActiveIfNeeded(false);
        AlkawaDebug.Log(ELogCategory.UI, OnCloseMessage);
    }

    public virtual void Open(UIBaseParameters parameters = null)
    {
        Parameters = parameters;   
        gameObject.SetActiveIfNeeded(true);
        AlkawaDebug.Log(ELogCategory.UI, OnOpenMessage);
    }
}