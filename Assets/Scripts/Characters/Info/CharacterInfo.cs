using System;
using Sirenix.OdinInspector;

[Serializable]
public class CharacterInfo
{
    [ShowInInspector, ReadOnly] public Cell Cell { get; set; }
    [ShowInInspector, ReadOnly] public int Speed { get; set; }  
    [ShowInInspector, ReadOnly] public bool LockSkill {get; set;}
    [ShowInInspector, ReadOnly] public CharacterAttributes Attributes { get; set; }
}