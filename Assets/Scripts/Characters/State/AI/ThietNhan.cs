using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThietNhan : AICharacter
{
    private int _moveCountTN = 0;
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new AIMoveState(this),
            new AIDamageTakenState(this),
            new ThietNhan_SkillState(this));
    }
    
    public override void SetMainCharacter()
    {
        base.SetMainCharacter();
        _moveCountTN = 0;
    }
    
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            Info.Speed = Info.Cell.CellPosition == new Vector2Int(6, 8) ? 8 : 7;
        }
        else
        {
            base.SetSpeed();
        }
    }
    
    protected override bool TryMoving()
    {
        if (_moveCountTN >= 1) return false;
        return base.TryMoving();
    }

    public override void TryMoveToCell(Cell cell)
    {
        base.TryMoveToCell(cell);
        _moveCountTN++;
    }
}