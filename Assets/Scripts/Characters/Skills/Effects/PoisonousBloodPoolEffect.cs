using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class PoisonousBloodPoolEffect : EffectData
{
    [NonSerialized] public List<Cell> impacts = new();
    public List<EffectData> effects = new();
    
    // Thêm trường để lưu vị trí cell thay vì reference trực tiếp
    public List<Vector2Int> impactPositions = new();
    
    // Thêm trường để lưu loại hoa gốc
    public EffectType sourceFlowerType;

    public override void OnBeforeSave()
    {
        base.OnBeforeSave();
        impactPositions.Clear();
        foreach (var cell in impacts)
        {
            impactPositions.Add(cell.CellPosition);
        }
    }

    public override void OnAfterLoad(MapManager mapManager)
    {
        base.OnAfterLoad(mapManager);
        
        // Khôi phục danh sách impacts từ impactPositions
        impacts.Clear();
        foreach (var pos in impactPositions)
        {
            var cell = mapManager.GetCell(pos);
            if (cell != null)
            {
                impacts.Add(cell);
                // Khôi phục hiệu ứng visual
                cell.poisonousBloodPool.enabled = true;
            }
        }
    }
}