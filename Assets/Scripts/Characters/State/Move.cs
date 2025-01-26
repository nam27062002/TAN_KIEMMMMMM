using DG.Tweening;

public class Move : CharacterState
{
    public Move(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Move";
    public override void OnEnter()
    {
        base.OnEnter();
        HandleMovement();
    }

    private void HandleMovement()
    {
        ReleaseFacing();
        Character.characterInfo.Cell.HideFocus();
        Character.characterInfo.MoveAmount += Character.characterInfo.MoveCells.Count;
        var moveSequence = DOTween.Sequence();
        float currentX = Transform.position.x;
        foreach (var cell in Character.characterInfo.MoveCells)
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
            SetCell(Character.characterInfo.MoveCells[^1]);
            Character.characterInfo.Cell.ShowFocus();
            OnFinishAction(this);
        });
        
    }
    
    public override void OnExit()
    {
        
    }
}