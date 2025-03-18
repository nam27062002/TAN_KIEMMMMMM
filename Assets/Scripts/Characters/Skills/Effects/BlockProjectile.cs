using System;
using UnityEngine;

[Serializable]
public class BlockProjectile : EffectData
{
     [NonSerialized]  public Cell targetCell;
     public Vector2Int position;
}