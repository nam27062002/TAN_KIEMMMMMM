﻿using System;
using DG.Tweening;

public class MoveState : CharacterState
{
    public MoveState(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Move";
    
    public override void OnEnter(StateParams stateParams = null)
    {
        GpManager.SetInteract(false);
        base.OnEnter(stateParams);
        HandleMovement((MoveStateParams)stateParams);
    }

    private void HandleMovement(MoveStateParams stateParams)
    {
        Character.UnRegisterCell();
        Character.HideMoveRange();
        ReleaseFacing();
        Character.Info.Cell.HideFocus();
        var moveAmount = stateParams.MoveCells.Count - 1;
        Character.Info.MoveAmount += moveAmount;
        Character.Info.HandleMoveAmountChanged(moveAmount);
        if (Character.IsMainCharacter)
        {
            GpManager.SetMainCell(null);
        }
        var moveSequence = DOTween.Sequence();
        float currentX = Transform.position.x;
        foreach (var cell in stateParams.MoveCells)
        {
            var targetPos = cell.transform.position;
            targetPos.y += Character.characterConfig.characterHeight / 2f;
            targetPos.z = targetPos.y;
            PlayAnim(AnimationParameterNameType.MoveLeft);
            PlayAnim(cell.transform.position.x > currentX ? AnimationParameterNameType.MoveRight :
                AnimationParameterNameType.MoveLeft);
            currentX = cell.transform.position.x;
            moveSequence.Append(Transform.DOMove(targetPos, 0.5f).SetEase(Ease.Linear));
        }
        
        moveSequence.OnComplete(() =>
        {
            OnReachToTarget(Character.Info.Cell, stateParams.MoveCells[^1]);
            if (Character.IsMainCharacter)
            {
                GpManager.SetMainCell(Character.Info.Cell);
            }
        });
        
    }
    
    public override void OnExit()
    {
        
    }
    
    protected virtual void OnReachToTarget(Cell from, Cell to)
    {
        SetCell(to);
        Character.Info.Cell.ShowFocus();
        GpManager.SetInteract(true);
        GpManager.UpdateAllFacing();
        Character.ChangeState(ECharacterState.Idle);
    }
}
