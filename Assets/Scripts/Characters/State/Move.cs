

using UnityEngine;

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
        // Character.PlayAnim(AnimationParameterNameType.MoveLeft);
    }

    private void HandleMovement()
    {
        
    }
    
    public override void OnExit()
    {
        
    }
}