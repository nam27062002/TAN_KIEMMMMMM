using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Flags]
public enum TutorialType
{
    None = 0,
    Arrow = 1 << 0,
    Menu = 1 << 1
}

[Serializable]
public class TutorialConfig
{
    [ListDrawerSettings(Expanded = true)]
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
}