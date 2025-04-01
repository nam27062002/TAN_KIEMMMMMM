using System;
using UnityEngine;

[Serializable]
public class BlockProjectile : EffectData
{
     [NonSerialized]  public Cell targetCell;
     public Vector2Int position;

     public override void OnAfterLoad(MapManager mapManager)
     {
          base.OnAfterLoad(mapManager);
          
          // Khôi phục targetCell từ position
          targetCell = mapManager.GetCell(position);
          if (targetCell != null)
          {
               // Khôi phục hiệu ứng visual
               targetCell.SetMainProjectile();
          }
     }
}