using System;
using UnityEngine;

[Serializable]
public class CharacterConfig
{
    public float characterHeight = 1f;
    public string characterName;
    public Sprite characterIcon;
    public Sprite slideBarIcon;
    public Sprite characterNoBgIcon;
    [TextArea(10, 20)]
    public string story;
    public CharacterAttributes characterAttributes;
    [Space]
    public SerializableDictionary<SkillTurnType, int> actionPoints = new();
    public bool hasToggle;
    public DirectionType moveDirection;
}

[Flags, Serializable]
public enum DirectionType
{
    None = 1 << 0,
    // Up = 1 << 1,
    // Down = 1 << 2,
    Left = 1 << 3,
    Right = 1 << 4,
    UpRight = 1 << 5,
    UpLeft = 1 << 6,
    DownLeft = 1 << 7,
    DownRight = 1 << 8,
    All = 1 << 9
}