using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class SkillInfo
{
    [HorizontalGroup("Skill Info", Width = 64)] 
    [PreviewField(64)] 
    public Sprite icon;

    [HorizontalGroup("Skill Info")]
    [VerticalGroup("Skill Info/Details")] 
    [LabelWidth(75)] 
    public string name;

    [HorizontalGroup("Skill Info")]
    [VerticalGroup("Skill Info/Details")] 
    [LabelWidth(75)] 
    public SkillIndex skillIndex;
    
    [VerticalGroup("Skill Info/Details")]
    [LabelWidth(75)] 
    [GUIColor(0.2f, 0.6f, 1f)]
    public int mpCost;

    [VerticalGroup("Skill Info/Details")]
    [LabelWidth(75)]
    public int range;

    [VerticalGroup("Skill Info/Details")]
    [LabelWidth(125)]
    [ToggleLeft] 
    public bool isDirectionalSkill;
    
    [VerticalGroup("Skill Info/Details")]
    [LabelWidth(125)]
    [ToggleLeft] 
    public bool canOverrideSetTargetCharacters;
    
    public DamageTargetType damageType;
    public string description;
}

