using UnityEngine;

public class ThietNhan : AICharacter
{
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            characterInfo.Speed = characterInfo.Cell.CellPosition == new Vector2Int(6, 8) ? 8 : 7;
        }
        else
        {
            base.SetSpeed();
        }
    }
}