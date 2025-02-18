using UnityEngine;

public class HoacLienHuong : PlayerCharacter
{
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new PlayerDamageTakenState(this),
            new HoacLienHuong_SkillState(this));
    }
    
    protected override void SetSpeed()
    {
        base.SetSpeed();
        Info.Speed = 100;
    }
    
    protected override bool CanBlockSkill(DamageTakenParams damageTakenParams)
    {
        if (base.CanBlockSkill(damageTakenParams)) return true;
        var path = MapManager.FindShortestPath(damageTakenParams.SkillStateParams.Source.Info.Cell, _skillStateParams.TargetCell);
        var canDodge = path.Count > damageTakenParams.SkillStateParams.SkillInfo.range;
        Debug.Log($"Khoảng cách hiện tại = {path.Count} | Khoảng cách skill = {damageTakenParams.SkillStateParams.SkillInfo.range} => né = {canDodge}");
        return canDodge;
    }
}