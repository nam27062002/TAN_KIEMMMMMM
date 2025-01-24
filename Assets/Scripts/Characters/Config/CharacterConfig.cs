using System;
using UnityEngine;

[Serializable]
public class CharacterConfig
{
    public float characterHeight = 1f;
    public string characterName;
    public Sprite characterIcon;
    public RollData damageRollData;
    public CharacterAttributes characterAttributes;
    public SerializableDictionary<SkillType, int> actionPoints = new();
}