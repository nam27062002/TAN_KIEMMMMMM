using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AICharacter : Character
{
    public override Type Type => Type.AI;
    protected Character Enemy;

    protected int MoveCount = 0;

    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new AIMoveState(this),
            new AIDamageTakenState(this),
            new AISkillState(this));
    }
    
    public override void SetMainCharacter()
    {
        base.SetMainCharacter();
        if (Info.Cell.mainShieldCell != null)
        {
            var damage = Roll.RollDice(1, 4, 0);
            Info.HandleDamageTaken(-damage, null);
            Debug.Log($"[{characterConfig.characterName}] Hiểu Nhật Quang Lâm: damage = 1d4 = {damage}");
        }
    }
    
    public void HandleAIPlayCoroutine()
    {
        Invoke(nameof(HandleAIPlay), 1f);
    }

    public virtual void HandleAIPlay()
    {
        AlkawaDebug.Log(ELogCategory.AI,"HandleAIPlay");
        Info.GetMoveRange(); // TODO: Clean code
        if (!TryCastSkill())
        {
            if (!TryMoving() || MoveCount >= 2)
            {
                MoveCount = 0;
                GameplayManager.Instance.HandleEndTurn("Hết trường hợp");
            }
        }
    }
    
    protected virtual bool TryMoving()
    {
        if (Info.GetMoveRange() <= 0) return false;
        List<Cell> cells = GpManager.MapManager.GetCellsWalkableInRange(Info.Cell, Info.GetMoveRange(), characterConfig.moveDirection);
        if (cells.Count == 0) return false;
        List<Cell> enemyCells = GpManager.Players.Select(item => item.Info.Cell).ToList();

        Cell targetCell = null;
        int minDistance = int.MaxValue;
        foreach (var cell in cells)
        {
            foreach (var enemyCell in enemyCells)
            {
                var p = MapManager.FindShortestPath(cell, enemyCell);
                if (p != null && p.Count < minDistance)
                {
                    minDistance = p.Count;
                    targetCell = cell;
                }
            }
        }
        if (targetCell == null) return false;
        var path = GpManager.MapManager.FindPath(Info.Cell, targetCell);
        TryMoveToCell(path);
        MoveCount++;
        AlkawaDebug.Log(ELogCategory.AI,$"move to cell: {targetCell.CellPosition}");
        return true;
    }
    
    protected bool TryCastSkill()
    {
        AlkawaDebug.Log(ELogCategory.AI,"TryCastSkill");
         var skillType = GpManager.GetSkillTurnType(this);
         List<SkillInfo> skills = GetSkillInfos(skillType);
        
         for (int i = 0; i < skills.Count; i++)
         {
             if (Info.CanCastSkill(skills[i]) && skills[i].isDirectionalSkill && skills[i].damageType.HasFlag(DamageTargetType.Enemies))
             {
                 var enemiesInRange = GpManager.GetEnemiesInRange(this, skills[i].range, skills[i].directionType);
                 if (enemiesInRange.Count > 0)
                 {
                     Enemy = enemiesInRange[0];
                     HandleCastSkill(skills[i], new List<Character> {Enemy});
                     AlkawaDebug.Log(ELogCategory.AI,$"HandleAICastSkill: {skills[i].name}");
                     return true;
                 }
             }
         }
        return false;
    }
}

