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
        
        // Kiểm tra có bị hiệu ứng Taunt không
        var tauntEffect = Info.EffectInfo.Effects.FirstOrDefault(e => e.effectType == EffectType.Taunt);
        Character tauntSource = tauntEffect?.Actor;
        
        List<Cell> cells = GpManager.MapManager.GetCellsWalkableInRange(Info.Cell, Info.GetMoveRange(), characterConfig.moveDirection);
        if (cells.Count == 0) return false;
        
        Cell targetCell = null;
        int minDistance = int.MaxValue;
        
        // Nếu dính hiệu ứng Taunt, ưu tiên di chuyển về phía nguồn gây Taunt
        if (tauntSource != null)
        {
            AlkawaDebug.Log(ELogCategory.AI, $"Đang bị hiệu ứng Taunt từ {tauntSource.characterConfig.characterName}");
            foreach (var cell in cells)
            {
                var p = MapManager.FindShortestPath(cell, tauntSource.Info.Cell);
                if (p != null && p.Count < minDistance)
                {
                    minDistance = p.Count;
                    targetCell = cell;
                }
            }
        }
        else
        {
            // Xử lý di chuyển như bình thường nếu không bị Taunt
            List<Cell> enemyCells = GpManager.Players.Select(item => item.Info.Cell).ToList();
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
        
        // Kiểm tra có bị hiệu ứng Taunt không
        var tauntEffect = Info.EffectInfo.Effects.FirstOrDefault(e => e.effectType == EffectType.Taunt);
        Character tauntSource = tauntEffect?.Actor;
        
        // Danh sách lưu trữ các cặp (skill, danh sách kẻ địch trong tầm)
        List<(SkillInfo skill, List<Character> enemies)> validSkills = new List<(SkillInfo, List<Character>)>();
        
        // Tìm tất cả skill thỏa mãn điều kiện
        foreach (var skill in skills)
        {
            if (Info.CanCastSkill(skill) && skill.isDirectionalSkill && skill.damageType.HasFlag(DamageTargetType.Enemies))
            {
                var enemiesInRange = GpManager.GetEnemiesInRange(this, skill.range, skill.directionType);
                
                // Nếu bị Taunt, chỉ tấn công nguồn gây Taunt nếu có thể
                if (tauntSource != null)
                {
                    if (enemiesInRange.Contains(tauntSource))
                    {
                        validSkills.Add((skill, new List<Character> { tauntSource }));
                    }
                }
                else if (enemiesInRange.Count > 0)
                {
                    validSkills.Add((skill, enemiesInRange));
                }
            }
        }
        
        // Nếu có skill thỏa mãn, chọn một skill
        if (validSkills.Count > 0)
        {
            // Chọn ngẫu nhiên một index trong danh sách skill thỏa mãn
            int randomIndex = UnityEngine.Random.Range(0, validSkills.Count);
            var selectedSkill = validSkills[randomIndex];
            
            // Với hiệu ứng Taunt, Enemy luôn là nguồn gây Taunt
            if (tauntSource != null && selectedSkill.enemies.Contains(tauntSource))
            {
                Enemy = tauntSource;
                AlkawaDebug.Log(ELogCategory.AI,$"Tấn công nguồn gây Taunt: {tauntSource.characterConfig.characterName}");
            }
            else if (GameplayManager.Instance.IsTutorialLevel)
            {
                // Trong tutorial level, tấn công nhân vật gần nhất thay vì ngẫu nhiên
                Enemy = FindNearestEnemy(selectedSkill.enemies);
                AlkawaDebug.Log(ELogCategory.AI, $"Tutorial - Tấn công nhân vật gần nhất: {Enemy.characterConfig.characterName}");
            }
            else
            {
                // Chọn kẻ địch ngẫu nhiên trong tầm nếu không trong tutorial
                int randomEnemyIndex = UnityEngine.Random.Range(0, selectedSkill.enemies.Count);
                Enemy = selectedSkill.enemies[randomEnemyIndex];
            }
            
            // Thực hiện skill
            HandleCastSkill(selectedSkill.skill, new List<Character> {Enemy});
            AlkawaDebug.Log(ELogCategory.AI,$"HandleAICastSkill: {selectedSkill.skill.name} targeting {Enemy.characterConfig.characterName}");
            return true;
        }
        
        return false;
    }
    
    // Phương thức mới để tìm nhân vật gần nhất
    private Character FindNearestEnemy(List<Character> enemies)
    {
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        
        Character nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var enemy in enemies)
        {
            // Tính khoảng cách thực tế giữa hai nhân vật
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            // Hoặc sử dụng khoảng cách trên grid thông qua pathfinding
            var path = MapManager.FindShortestPath(Info.Cell, enemy.Info.Cell);
            int pathLength = path?.Count ?? int.MaxValue;
            
            // Ưu tiên khoảng cách trên grid nếu có
            float effectiveDistance = pathLength != int.MaxValue ? pathLength : distance;
            
            if (effectiveDistance < minDistance)
            {
                minDistance = effectiveDistance;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
}

