public abstract class PlayerCharacter : Character
{
    public override Type Type => Type.Player;
    public override bool CanEndTurn => IsMainCharacter;
    
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new PlayerMoveState(this),
            new DamageTakenState(this),
            new SkillState(this));
    }
    
    public override void OnSelected()
    {
        base.OnSelected();
        
        if (GpManager.MainCharacter == this && !GpManager.IsTutorialLevel)
        {
            ShowMoveRange();
        }
    }
}