using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialConfig", menuName = "SO/Tutorial/TutorialConfig")]
public class TutorialConfig : ScriptableObject
{
    [ListDrawerSettings(ShowFoldout = true)]
    public List<TutorialData> tutorials = new List<TutorialData>();
    
    [Serializable]
    public class TutorialData
    {
        [EnumToggleButtons]
        [LabelText("Tutorial Types")]
        public TutorialType tutorialTypes;
        
        // Arrow Tutorial
        [ShowIf("@this.tutorialTypes.HasFlag(TutorialType.Arrow)")]
        [BoxGroup("Arrow Tutorial")]
        [LabelText("Arrow Position")]
        public Vector2 arrowPosition;

        [ShowIf("@this.tutorialTypes.HasFlag(TutorialType.Arrow)")]
        [BoxGroup("Arrow Tutorial")]
        [LabelText("Arrow Rotation")]
        public Quaternion arrowRotation;

        // Menu Tutorial
        [ShowIf("@this.tutorialTypes.HasFlag(TutorialType.Menu)")]
        [BoxGroup("Menu Tutorial")]
        [TextArea]
        [LabelText("Menu Text")]
        public string tutorialMenuText;
    }
    
    [Button("Find Longest Menu Text")]
    public void FindLongestMenuText()
    {
        if (tutorials == null || tutorials == null || tutorials.Count == 0)
        {
            Debug.LogWarning("TutorialConfig is missing or empty.");
            return;
        }

        var longest = tutorials
            .Where(t => t.tutorialTypes.HasFlag(TutorialType.Menu) && !string.IsNullOrEmpty(t.tutorialMenuText))
            .OrderByDescending(t => t.tutorialMenuText.Length)
            .FirstOrDefault();

        Debug.Log(longest != null
            ? $"Longest Menu Text ({longest.tutorialMenuText.Length} chars):\n{longest.tutorialMenuText}"
            : "No tutorial with Menu text found.");
    }
}

[Flags, Serializable]
public enum TutorialType
{
    None = 0,
    Arrow = 1 << 0,
    Menu = 1 << 1
}