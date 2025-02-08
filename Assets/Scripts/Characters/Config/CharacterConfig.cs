using System;
using UnityEngine;

[Serializable]
public class CharacterConfig
{
    public float characterHeight = 1f;
    public string characterName;
    public float timeMoveToTarget = 0.5f;
    public Sprite characterIcon;
    public CharacterAttributes characterAttributes;
    public SerializableDictionary<SkillType, int> actionPoints = new();
}