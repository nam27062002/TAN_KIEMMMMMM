﻿using System.Collections.Generic;

public class ThietNhan_SkillState : AISkillState
{
    public ThietNhan_SkillState(Character character) : base(character)
    {
    }

    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        int baseDamage = GetBaseDamage();
        int skillDamage = Roll.RollDice(1, 4, 0);
        int totalDamage = baseDamage + skillDamage;

        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: Skill Damage = 1d4 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Ném Đá: damage = {baseDamage} + {skillDamage} = {totalDamage}");

        var friends = GpManager.MapManager.GetAllTypeInRange(Info.Cell, CharacterType.ThietNhan, 1);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: có {friends.Count} Thiết Nhân đứng sát");

        totalDamage = ProcessFriendAttacks(friends, totalDamage);

        return new DamageTakenParams
        {
            Damage = totalDamage,
            Effects = new List<EffectData>
            {
                new PoisonEffectData
                {
                    EffectType = EffectType.ThietNhan_Poison,
                    Duration = EffectConfig.DebuffRound,
                    Damage = new RollData
                    {
                        rollTime = 1,
                        rollValue = 4,
                        add = 0
                    }
                },
                new ChangeStatEffect
                {
                    EffectType = EffectType.ThietNhan_ReduceMoveRange,
                    Duration = EffectConfig.DebuffRound,
                    Value = 2,
                },
                new EffectData
                {
                    EffectType = EffectType.ThietNhan_BlockAP,
                    Duration = EffectConfig.DebuffRound,
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        int baseDamage = GetBaseDamage();
        int skillDamage = Roll.RollDice(1, 4, 0);

        var friends = GpManager.MapManager.GetAllTypeInRange(Info.Cell, CharacterType.ThietNhan, 3);
        skillDamage += friends.Count;
        int totalDamage = baseDamage + skillDamage;

        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: có {friends.Count} Thiết Nhân đứng cạnh trong 3 ô");
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Ném Đá: Skill Damage = 1d4 + 1 * {friends.Count} = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Ném Đá: damage = {baseDamage} + {skillDamage} = {totalDamage}");

        totalDamage = ProcessFriendAttacks(friends, totalDamage);

        return new DamageTakenParams
        {
            Damage = totalDamage,
            Effects = new List<EffectData>
            {
                new EffectData
                {
                    EffectType = EffectType.Poison,
                    Duration = EffectConfig.DebuffRound,
                },
                new EffectData
                {
                    EffectType = EffectType.Immobilize,
                    Duration = EffectConfig.DebuffRound,
                }
            }
        };
    }

    
    //===================== SKILL 3 =====================
    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character)
    {
        int baseDamage = GetBaseDamage();
        int skillDamage = Roll.RollDice(1, 4, 0);
        int totalDamage = baseDamage + skillDamage;
        
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Lây Nhiễm: Skill Damage = 1d4 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL,
            $"[{CharName}] Lây Nhiễm: damage = {baseDamage} + {skillDamage} = {totalDamage}");
        return new DamageTakenParams()
        {
            Damage = totalDamage,
            Effects = new List<EffectData>()
            {
                new()
                {
                    EffectType = EffectType.ThietNhan_Infected,
                    Duration = EffectConfig.DebuffRound,
                }
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character character)
    {
        return new DamageTakenParams { Damage = GetBaseDamage() };
    }
    
    private int ProcessFriendAttacks(IEnumerable<Character> friends, int totalDamage)
    {
        foreach (var friend in friends)
        {
            int roll = Roll.RollDice(1, 20, 0);
            AlkawaDebug.Log(ELogCategory.SKILL, "----------------------------------------------------------");

            if (roll >= 10)
            {
                var animName = GetAnimByIndex(_skillStateParams.SkillInfo.skillIndex);
                friend.StateMachine.GetCurrentState.PlayAnim(animName);
                AlkawaDebug.Log(ELogCategory.SKILL,
                    $"[{CharName}] Ném Đá: 1d20 = {roll} >= 10 => có thể cùng tấn công");

                int friendBaseDamage = GetBaseDamage();
                int friendSkillDamage = Roll.RollDice(1, 4, 0);
                int friendTotalDamage = friendBaseDamage + friendSkillDamage;
                totalDamage += friendTotalDamage;

                AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: Skill Damage = 1d4 = {friendSkillDamage}");
                AlkawaDebug.Log(ELogCategory.SKILL,
                    $"[{CharName}] Ném Đá: damage = {friendBaseDamage} + {friendSkillDamage} = {friendTotalDamage}");
            }
            else
            {
                AlkawaDebug.Log(ELogCategory.SKILL,
                    $"[{CharName}] Ném Đá: 1d20 = {roll} < 10 => Không thể cùng tấn công");
            }
        }

        return totalDamage;
    }
}