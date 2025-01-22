using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class SkillConfig
{
    [TabGroup("Internal Skill")] public List<SkillInfo> internalSkill = new();
    [TabGroup("Movement Skill")] public List<SkillInfo> movementSkill = new();
    [TabGroup("Combat Skill")] public List<SkillInfo> combatSkill = new();

    public Dictionary<SkillType, List<SkillInfo>> SkillConfigs = new();

    public void SetSkillConfigs()
    {
        if (SkillConfigs == null || SkillConfigs.Count == 0)
        {
            SkillConfigs = new Dictionary<SkillType, List<SkillInfo>>
            {
                [SkillType.InternalSkill] = internalSkill,
                [SkillType.MovementSkill] = movementSkill,
                [SkillType.CombatSkill] = combatSkill
            };
        }
    }

    public void OnValidate()
    {
        for (var i = 0; i < internalSkill.Count; i++)
        {
            internalSkill[i].skillIndex = i;
            movementSkill[i].skillIndex = i;
            combatSkill[i].skillIndex = i;
        }
    }
}