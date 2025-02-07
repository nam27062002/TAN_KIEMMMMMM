using UnityEngine;

public class ThietNhan : AICharacter
{
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            CharacterInfo.Speed = CharacterInfo.Cell.CellPosition == new Vector2Int(6, 8) ? 8 : 7;
        }
        else
        {
            base.SetSpeed();
        }
    }
}