using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThietNhan : AICharacter
{
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
        MoveCount = 0;
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
#if UNITY_EDITOR
        Info.Speed = Info.Cell.CellPosition == new Vector2Int(2, 6) ? 800 : 600;
#endif
    }
    
    protected override bool TryMoving()
    {
        if (MoveCount >= 1) return false;
        
        var tauntEffect = Info.EffectInfo.Effects.FirstOrDefault(e => e.effectType == EffectType.Taunt);
        if (tauntEffect != null)
        {
            return base.TryMoving();
        }
        
        return base.TryMoving();
    }

    public override void TryMoveToCell(Cell cell)
    {
        base.TryMoveToCell(cell);
        MoveCount++;
    }
}