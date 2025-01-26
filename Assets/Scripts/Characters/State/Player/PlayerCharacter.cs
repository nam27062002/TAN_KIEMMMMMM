public abstract class PlayerCharacter : Character
{
    public override Type Type => Type.Player;
    
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new DamageTakenState(this),
            new SkillState(this));
    }
    
    public override void OnSelected()
    {
        base.OnSelected();
        if (CharacterManager.MainCharacter == this && !GameplayManager.Instance.IsTutorialLevel)
        {
            CharacterManager.ShowMoveRange();
        }
    }
}