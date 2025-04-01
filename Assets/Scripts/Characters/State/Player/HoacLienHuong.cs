using System;
using UnityEngine;

public class HoacLienHuong : PlayerCharacter
{
    [SerializeField] private float goldenAPChance = 0.25f;
    [NonSerialized] public Cell CurrentShield;
    public Vector2Int CurrentShieldPosition; // Vị trí của shield để lưu trong save
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
    }
    
    protected override bool CanBlockSkill(DamageTakenParams damageTakenParams)
    {
        if (base.CanBlockSkill(damageTakenParams)) return true;
        var path = MapManager.FindShortestPath(damageTakenParams.SkillStateParams.Source.Info.Cell, SkillStateParams.TargetCell);
        if (path == null) return false;
        var canDodge = path.Count > damageTakenParams.SkillStateParams.SkillInfo.range;
        Debug.Log($"Khoảng cách hiện tại = {path.Count} | Khoảng cách skill = {damageTakenParams.SkillStateParams.SkillInfo.range} => né = {canDodge}");
        return canDodge;
    }
    
    public override int GetSkillActionPoints(SkillTurnType skillTurnType)
    {
        if (skillTurnType == SkillTurnType.EnemyTurn)
        {
            float roll = UnityEngine.Random.value;
            int ap = roll < 0.25f ? 1 : 2;
            AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Lượt địch - Kết quả roll AP: {roll} => AP = {ap}");
            
            if(roll < goldenAPChance)
            {
                goldenAPChance = 0.25f;
                AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Kích hoạt AP vàng! (Xác suất: {goldenAPChance}, Roll: {roll})");
                AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Xác suất AP vàng được reset: {goldenAPChance}");
            }
            else
            {
                AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Kích hoạt AP đỏ. (Xác suất: {goldenAPChance}, Roll: {roll})");
                goldenAPChance = Mathf.Min(goldenAPChance + 0.10f, 1f);
                AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Xác suất AP vàng tăng lên: {goldenAPChance}");
            }
            return ap;
        }
        
        return base.GetSkillActionPoints(skillTurnType);
    }
}