using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

[Serializable]
public class SkillConfig
{
    [TabGroup("My Turn Skill")] public List<SkillInfo> internalSkill = new();
    [TabGroup("Teammate Skill")] public List<SkillInfo> movementSkill = new();
    [TabGroup("Enemy Skill")] public List<SkillInfo> combatSkill = new();
    
    public Dictionary<SkillTurnType, List<SkillInfo>> SkillConfigs = new();
    public SkillInfo passtiveSkill1;
    public SkillInfo passtiveSkill2;

    [ShowInInspector] public bool HasPasstiveSkill1 => !string.IsNullOrEmpty(passtiveSkill1.description); 
    [ShowInInspector] public bool HasPasstiveSkill2 => !string.IsNullOrEmpty(passtiveSkill2.description);
    [ShowInInspector] public bool HasInternalSkill => internalSkill.Any(p => !string.IsNullOrEmpty(p.description));
    [ShowInInspector] public bool HasMovementSkill => movementSkill.Any(p => !string.IsNullOrEmpty(p.description));
    [ShowInInspector] public bool HasCombatSkill => combatSkill.Any(p => !string.IsNullOrEmpty(p.description));
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