using System;
using System.Collections.Generic;

public abstract class AICharacter : Character
{
    public override Type Type => Type.AI;
    protected Character Enemy;

    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new AIMoveState(this),
            new DamageTakenState(this),
            new AISkillState(this));
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
            if (!TryMoving())
            {
                GameplayManager.Instance.HandleEndTurn();
            }
        }
    }
    
    protected bool TryMoving()
    {
        if (Info.GetMoveRange() <= 0) return false;
        var cells = GpManager.MapManager.GetCellsWalkableInRange(Info.Cell, Info.GetMoveRange(), characterConfig.moveDirection);
        if (cells.Count == 0) return false;
        var random = new System.Random();
        var randomCell = cells[random.Next(cells.Count)];
        var path = GpManager.MapManager.FindPath(Info.Cell, randomCell);
        TryMoveToCell(path);
        AlkawaDebug.Log(ELogCategory.AI,$"move to cell: {randomCell.CellPosition}");
        return true;
    }
    
    private bool TryCastSkill()
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