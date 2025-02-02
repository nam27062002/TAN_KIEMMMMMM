using System;
using System.Collections.Generic;
using UnityEngine;

public class AICharacter : Character
{
    public override Type Type => Type.AI;
    private Character _enemy;

    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new AIMoveState(this),
            new DamageTakenState(this),
            new SkillState(this));
    }
    
    public override void SetMainCharacter()
    {
        base.SetMainCharacter();
        Invoke(nameof(HandleAIPlay), 1f);
    }

    private void HandleAIPlay()
    {
        //AlkawaDebug.Log("NT - HandleAIPlay");
        if (!TryCastSkill())
        {
            if (!TryMoving())
            {
                GameplayManager.Instance.HandleEndTurn();
            }
        }
    }
    
    private bool TryMoving()
    {
        if (characterInfo.GetMoveRange() <= 0) return false;
        var cells = CharacterManager.MapManager.GetCellsWalkableInRange(characterInfo.Cell, characterInfo.GetMoveRange());
        if (cells.Count == 0) return false;
        var random = new System.Random();
        var randomCell = cells[random.Next(cells.Count)];
        var path = CharacterManager.MapManager.FindPath(characterInfo.Cell, randomCell);
        MoveCharacter(path);
        //AlkawaDebug.Log($"Gameplay: AI move to cell: {randomCell.CellPosition}");
        return true;
    }
    
    public override void MoveCharacter(List<Cell> cells)
    {
        base.MoveCharacter(cells);
        
    }
    
    private bool TryCastSkill()
    {
        //AlkawaDebug.Log("NT - TryCastSkill");
        var skillType = CharacterManager.GetSkillType(this);
        List<SkillInfo> skills = GetSkillInfos(skillType);
        
        for (int i = 0; i < skills.Count; i++)
        {
            if (characterInfo.CanCastSkill(skills[i], skillType) && skills[i].isDirectionalSkill && skills[i].damageType.HasFlag(DamageTargetType.Enemies))
            {
                var enemiesInRange = base.CharacterManager.GetEnemiesInRange(this, skills[i].range);
                if (enemiesInRange.Count > 0)
                {
                    _enemy = enemiesInRange[0];
                    _enemy.OnEndAnimEventHandler += EnemyOnOnEndAnimEventHandler;
                    //AlkawaDebug.Log($"NT - HandleAICastSkill: {i}");
                    GameplayManager.Instance.HandleCastSkill(_enemy, skills[i]);
                    return true;
                }
            }
        }
        return false;
    }

    private void EnemyOnOnEndAnimEventHandler(object sender, EventArgs e)
    {
        //AlkawaDebug.Log("EnemyOnOnEndAnimEventHandler");
        _enemy.OnEndAnimEventHandler -= EnemyOnOnEndAnimEventHandler;
        GameplayManager.Instance.HandleEndTurn();
    }
    
}