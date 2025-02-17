using System.Collections.Generic;
using UnityEngine;

public class ThietNhan_SkillState : AISkillState
{
    public ThietNhan_SkillState(Character character) : base(character)
    {
    }
    
    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 0);
        var realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: Skill Damage = 1d4 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: damage = {baseDamage} + {skillDamage} = {realDamage}");
        
        var friends = GpManager.MapManager.GetAllTypeInRange(Info.Cell, CharacterType.ThietNhan, 10);
        AlkawaDebug.Log(ELogCategory.SKILL,$"[{Character.characterConfig.characterName}] Ném Đá: có {friends.Count} Thiết Nhân đứng sát");
        foreach (var item in friends)
        {
            var roll = Roll.RollDice(1, 20, 0);
           
            if (roll >= 10 || true)
            {
                var animName = GetAnimByIndex(_skillStateParams.SkillInfo.skillIndex);
                item.StateMachine.GetCurrentState.PlayAnim(animName);
                AlkawaDebug.Log(ELogCategory.SKILL, $"----------------------------------------------------------");
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: 1d20 = {roll} < 10 => có thể cùng tấn công");
                
                var baseDamage1 = GetBaseDamage();
                var skillDamage1 = Roll.RollDice(1, 4, 0);
                var realDamage1 = baseDamage1 + skillDamage1;
                
                realDamage += realDamage1;
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: Skill Damage = 1d4 = {baseDamage1}");
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: damage = {baseDamage1} + {skillDamage1} = {realDamage1}");
            }
            else
            {
                AlkawaDebug.Log(ELogCategory.SKILL, $"----------------------------------------------------------");
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: 1d20 = {roll} < 10 => Không thể cùng tấn công");
            }
        }

        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects =  new Dictionary<EffectType, int>()
            {
                { EffectType.Poison, EffectConfig.DebuffRound}
            }
        };
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 0);
        var realDamage = baseDamage + skillDamage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: Skill Damage = 1d4 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] Ném Đá: damage = {baseDamage} + {skillDamage} = {realDamage}");
        return new DamageTakenParams
        {
            Damage = realDamage,
            Effects =  new Dictionary<EffectType, int>()
            {
                { EffectType.Poison, EffectConfig.DebuffRound}
            }
        };
    }
}