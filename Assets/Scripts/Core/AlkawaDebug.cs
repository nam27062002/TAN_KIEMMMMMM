#if UNITY_EDITOR
#define USE_DEBUG
#endif
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public enum ELogCategory
{
    NONE = 0,
    UI,
    CHARACTER,
    ANIMATION,
    MAP,
    ENGINE,
    AI,
    CONSOLE,
    GAMEPLAY,
}

public enum ELogSeverity
{
    INFO,
    WARNING,
    ERROR
}

public class AlkawaDebug
{
    private static readonly Dictionary<ELogCategory, string> CategoryColors = new Dictionary<ELogCategory, string>();
    private static readonly HashSet<ELogCategory> IgnoredCategories = new HashSet<ELogCategory>()
    {
        ELogCategory.NONE,
        ELogCategory.UI,
        // ELogCategory.CHARACTER,
        ELogCategory.ANIMATION,
        ELogCategory.MAP,
        ELogCategory.ENGINE,
        ELogCategory.AI,
        ELogCategory.GAMEPLAY,
    };
    
    static AlkawaDebug()
    {
        CategoryColors[ELogCategory.UI] = "#2196F3";        
        CategoryColors[ELogCategory.CHARACTER] = "#4CAF50";    
        CategoryColors[ELogCategory.ANIMATION] = "#9C27B0"; 
        CategoryColors[ELogCategory.MAP] = "#FF9800"; 
        CategoryColors[ELogCategory.ENGINE] = "#607D8B";    
        CategoryColors[ELogCategory.AI] = "#FF5722";      
        CategoryColors[ELogCategory.CONSOLE] = "#E91E63";
        CategoryColors[ELogCategory.GAMEPLAY] = "#FFEB3B";
    }

    [Conditional("USE_DEBUG")]
    public static void Log(ELogCategory cat, string msg, Object context = null)
    {
        if (IgnoredCategories.Contains(cat)) return;
        InternalLog(cat, ELogSeverity.INFO, msg, context);
    }

    [Conditional("USE_DEBUG")]
    private static void InternalLog(ELogCategory cat, ELogSeverity sev, string _msg, Object context)
    {
        string categoryPart = "";
        if (cat != ELogCategory.NONE)
        {
            string colorHex = CategoryColors.GetValueOrDefault(cat, "#FFFFFF");
            categoryPart = $"<color={colorHex}>[{cat}]</color> ";
        }

        string msg = $"{categoryPart}{_msg}";

        switch (sev)
        {
            case ELogSeverity.INFO:
                Debug.Log(msg, context);
                break;
            case ELogSeverity.WARNING:
                Debug.LogWarning(msg, context);
                break;
            case ELogSeverity.ERROR:
                Debug.LogError(msg, context);
                break;
        }
    }
}