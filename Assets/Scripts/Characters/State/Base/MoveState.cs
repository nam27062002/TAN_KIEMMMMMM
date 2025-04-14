using System;
using DG.Tweening;

public class MoveState : CharacterState
{
    public MoveState(Character self) : base(self)
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
        Self.UnRegisterCell();
        Self.HideMoveRange();
        ReleaseFacing();
        Self.Info.Cell.HideFocus();
        var moveAmount = stateParams.MoveCells.Count - 1;
        Self.Info.MoveAmount += moveAmount;
        Self.Info.HandleMoveAmountChanged(moveAmount);
        if (Self.IsMainCharacter)
        {
            GpManager.SetMainCell(null);
        }
        var moveSequence = DOTween.Sequence();
        float currentX = Transform.position.x;
        foreach (var cell in stateParams.MoveCells)
        {
            var targetPos = cell.transform.position;
            targetPos.y += Self.characterConfig.characterHeight / 2f;
            targetPos.z = targetPos.y;
            PlayAnim(AnimationParameterNameType.MoveLeft);
            PlayAnim(cell.transform.position.x > currentX ? AnimationParameterNameType.MoveRight :
                AnimationParameterNameType.MoveLeft);
            currentX = cell.transform.position.x;
            moveSequence.Append(Transform.DOMove(targetPos, 0.5f).SetEase(Ease.Linear));
        }
        
        moveSequence.OnComplete(() =>
        {
            OnReachToTarget(Self.Info.Cell, stateParams.MoveCells[^1]);
            if (Self.IsMainCharacter)
            {
                GpManager.SetMainCell(Self.Info.Cell);
            }
        });
        
    }
    
    public override void OnExit()
    {
        
    }
    
    protected virtual void OnReachToTarget(Cell from, Cell to)
    {
        SetCell(to);
        Self.Info.Cell.ShowFocus();
        GpManager.SetInteract(true);
        GpManager.UpdateAllFacing();
        Self.ChangeState(ECharacterState.Idle);
    }
}
