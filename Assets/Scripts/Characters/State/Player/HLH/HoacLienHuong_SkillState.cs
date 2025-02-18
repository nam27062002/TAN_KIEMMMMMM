using System.Linq;
using UnityEngine;

public class HoacLienHuong_SkillState : SkillState
{
    public HoacLienHuong_SkillState(Character character) : base(character)
    {
    }
    
    protected override void HandleCastSkill()
    {
        base.HandleCastSkill();
        if (_skillStateParams.SkillInfo.skillIndex == SkillIndex.ActiveSkill2)
        {
            MoveToCell(_skillStateParams.TargetCell, 0.5f);
        }
    }
    
    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(1, 4, 0);
        var totalDamage = baseDamage + skillDamage;
        Debug.Log($"Skill Damage = 1d4 = {skillDamage}");
        Debug.Log("Chưa tính số thi thể");
        Debug.Log($"Total Damage = {baseDamage} + {skillDamage} = {totalDamage}");
        return new DamageTakenParams
        {
            Damage = totalDamage,
        };   
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character character)
    {
        return new DamageTakenParams();
    }
    
    //===================== SKILL 2 =====================
    protected override void SetTargetCharacters_Skill2_MyTurn()
    {
        var path = GpManager.MapManager.FindShortestPath(Info.Cell, _skillStateParams.TargetCell);
        var targets = (from item in path where item.CellType == CellType.Character && item.Character.Type == Type.AI select item.Character).ToList();
        foreach (var item in targets)
        {
            AddTargetCharacters(item);
        }
    }

    protected override void SetTargetCharacters_Skill2_TeammateTurn()
    {
        var character = GpManager.GetNearestAlly(Character);
        AddTargetCharacters(character);
    }
        
}