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
        Character.characterInfo.Cell.CellType = CellType.Walkable;
        ReleaseFacing();
        Character.characterInfo.Cell.HideFocus();
        var moveAmount = stateParams.MoveCells.Count - 1;
        Character.characterInfo.MoveAmount += moveAmount;
        var moveSequence = DOTween.Sequence();
        float currentX = Transform.position.x;
        foreach (var cell in stateParams.MoveCells)
        {
            var targetPos = cell.transform.position;
            targetPos.y += Character.characterConfig.characterHeight / 2f;
            Character.PlayAnim(AnimationParameterNameType.MoveLeft);
            Character.PlayAnim(cell.transform.position.x > currentX ? AnimationParameterNameType.MoveRight :
                AnimationParameterNameType.MoveLeft);
            currentX = cell.transform.position.x;
            moveSequence.Append(Transform.DOMove(targetPos, 0.5f).SetEase(Ease.Linear));
        }
        
        moveSequence.OnComplete(() =>
        {
            SetCell(stateParams.MoveCells[^1]);
            Character.characterInfo.Cell.ShowFocus();
            GpManager.SetInteract(true);
            OnFinishAction();
        });
        
    }
    
    public override void OnExit()
    {
        
    }
}