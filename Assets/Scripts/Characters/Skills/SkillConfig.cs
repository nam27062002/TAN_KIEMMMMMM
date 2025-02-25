using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class SkillConfig
{
    [TabGroup("My Turn Skill")] public List<SkillInfo> internalSkill = new();
    [TabGroup("Teammate Skill")] public List<SkillInfo> movementSkill = new();
    [TabGroup("Enemy Skill")] public List<SkillInfo> combatSkill = new();

    public Dictionary<SkillTurnType, List<SkillInfo>> SkillConfigs = new();

    public void SetSkillConfigs()
    {
        if (SkillConfigs == null || SkillConfigs.Count == 0)
        {
            SkillConfigs = new Dictionary<SkillTurnType, List<SkillInfo>>
            {
                [SkillTurnType.MyTurn] = internalSkill,
                [SkillTurnType.TeammateTurn] = movementSkill,
                [SkillTurnType.EnemyTurn] = combatSkill
            };
        }
    }

    public void OnValidate()
    {
        for (var i = 0; i < internalSkill.Count; i++)
        {
            if (i < internalSkill.Count) internalSkill[i].skillIndex = (SkillIndex)i;
            if (i < movementSkill.Count) movementSkill[i].skillIndex = (SkillIndex)i;
            if (i < combatSkill.Count) combatSkill[i].skillIndex = (SkillIndex)i;
        }
    }
}

[Flags]
public enum DamageTargetType
{
    Self = 1 << 0,
    Team = 1 << 1,
    Enemies = 1 << 2,
    Move = 1 << 3,
    Walkable = 1 << 4,
}