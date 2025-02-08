using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class SkillConfig
{
    [TabGroup("Internal Skill")] public List<SkillInfo> internalSkill = new();
    [TabGroup("Movement Skill")] public List<SkillInfo> movementSkill = new();
    [TabGroup("Combat Skill")] public List<SkillInfo> combatSkill = new();

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
            internalSkill[i].skillIndex = (SkillIndex)i;
            movementSkill[i].skillIndex = (SkillIndex)i;
            combatSkill[i].skillIndex = (SkillIndex)i;
        }
    }
}

[Flags]
public enum DamageTargetType
{
    Self = 1 << 0,
    Team = 1 << 1,
    Enemies = 1 << 2,
}

[Flags]
public enum BuffType
{
    None = 0,
    IncreaseActionPoints = 1 << 0,
    IncreaseMoveRange = 1 << 1,
    BlockSkill = 1 << 2,
    CannotCrit = 1 << 3,
    ReduceAttackSize = 1 << 4,
    CannotMove = 1 << 6,
    Stun = 1 << 7,
    Parry = 1 << 8,
    ReduceActionPoints = 1 << 9,
    ReduceMoveRange = 1 << 10,
    Sleep = 1 << 11,
    LifeSteal = 1 << 12,
}

public enum EffectIndex
{
    None = 0,
    E_1_ChiBan,             // 1: cấm dùng nội lực
    E_2_ChiBreak,           // 2: chém mất nội lực (MP)
    E_3_LifeSteal,          // 3: hút máu
    E_4_Drunk,              // 4: say rượu, kiểm tra giấc ngủ
    E_5_Sleep,              // 5: stun cho tới khi nhận sát thương
    E_6_IncreaseStats,      // 6: tăng atk, crit, evasion, accuracy
    E_7_DecreaseStats,      // 7: giảm def, atk vật lý, accuracy, magic def
    E_8_Bleed,              // 8: sát thương theo hành động và di chuyển
    E_9_Prone,              // 9: nằm sấp, giảm move range
    E_10_Fear,              // 10: hiệu ứng sợ hãi
    E_11_Spike,             // 11: phản sát thương
    E_12_Poison,            // 12: sát thương theo lượt
    E_13_Rot,               // 13: giảm stat và kiểm tra poison
    E_14_Stun,              // 14: choáng
    E_15_Immobilize,        // 15: không thể di chuyển
    E_16_DecreaseResources, // 16: giảm move range, AP
    E_17_IncreaseResources, // 17: tăng move range, AP
    E_18_Heal,              // 18: hồi máu
    E_19_Cover,             // 19: nhận sát thương thay đồng đội
    E_20_Link,              // 20: chia sẻ sát thương nhận vào hoặc MP tiêu hao
    E_21_Block,             // 21: chặn sát thương, kỹ năng hoặc đòn đánh
    E_22_Summon,            // 22: triệu hồi unit
    E_23_Disarm,            // 23: làm rơi vũ khí
    E_24_Blink,             // 24: di chuyển nhanh không tốn tài nguyên
    E_25_Swap,              // 25: đổi chỗ với kẻ địch
    E_26_Shield,            // 26: tạo máu ảo
    E_27_Purify,            // 27: xóa debuffs
    E_28_Taunt,             // 28: khiêu khích
    E_29_Silence,           // 29: khóa kỹ năng
    E_30_Burn,              // 30: sát thương theo lượt (đốt cháy)
    E_31_Blind,             // 31: chặn crit, giảm tầm đánh
    E_32_DirectionLock,     // 32: chỉ có thể di chuyển 1 hướng
    E_33_HiveMind,          // 33: hiệu ứng tâm trí bầy đàn
    E_34_Immune,            // 34: miễn nhiễm sát thương
    E_35_Vulnerability,     // 35: luôn bị crit bởi một loại sát thương
    E_36_StealResource,     // 36: cướp tài nguyên
    E_37_Charm,             // 37: kiểm tra mê hoặc
    E_38_PressurePoint      // 38: điểm huyệt, immobilize và giảm phạm vi kỹ năng
}
