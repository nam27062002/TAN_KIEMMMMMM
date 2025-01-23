using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum HighlightType
{
    None,
    ActiveObject,
}
    
[Serializable]
public class Highlightable 
{
    [SerializeField] private HighlightType highlightType;
        
    [ShowIf("@highlightType == HighlightType.ActiveObject"), SerializeField] private GameObject highlightObject;
        
    public void Highlight()
    {
        switch (highlightType)
        {
            case HighlightType.ActiveObject:
                highlightObject.SetActive(true);
                break;
        }
    }

    public void Unhighlight()
    {
        switch (highlightType)
        {
            case HighlightType.ActiveObject:
                highlightObject.SetActive(false);
                break;
        }
    }
}