public class AICharacter : Character
{
    public override Type Type => Type.AI;
    
    public override void SetMainCharacter()
    {
        base.SetMainCharacter();
        Invoke(nameof(HandleAIPlay), 1f);
    }

    private void HandleAIPlay()
    {
        if (!TryCastSkill())
        {
            // if (!TryMoving())
            // {
            //     CharacterManager.OnEndTurn?.Invoke();
            // }
        }
    }
    
    private bool TryMoving()
    {
        // if (Info.GetMoveRange() <= 0) return false;
        // var cells = CharacterManager.MapManager.GetCellsWalkableInRange(Info.Cell, Info.GetMoveRange());
        // if (cells.Count == 0) return false;
        // var random = new System.Random();
        // var randomCell = cells[random.Next(cells.Count)];
        // var path = CharacterManager.MapManager.FindPath(Info.Cell, randomCell);
        // MoveCharacter(path, OnReachToDestination);
        // Debug.Log($"Gameplay: AI move to cell: {randomCell.CellPosition}");
        return true;
    }
    
    private bool TryCastSkill()
    {
        // SkillInfo skill = Config.skillInfo;
        //
        // for (int i = 0; i < skill.datas.Count; i++)
        // {
        //     if (Info.CanCastSkill(skill.datas[i]) && !skill.datas[i].isDirectionalSkill && skill.datas[i].damageTargetType.HasFlag(DamageTargetType.Enemies))
        //     {
        //         var enemiesInRange = CharacterManager.Instance.GetEnemiesInRange(this, skill.datas[i].range);
        //         if (enemiesInRange.Count > 0)
        //         {
        //             CharacterManager.OnEndAnimFeedback += OnAnimFeedbackFinished;
        //             CharacterManager.HandleCastSkill(enemiesInRange[0], i, skill.datas[i]);
        //             return true;
        //         }
        //     }
        // }
        return false;
    }
}