public abstract class PlayerCharacter : Character
{
    public override Type Type => Type.Player;
    public override bool CanEndTurn => IsMainCharacter || GpManager.MainCharacter.Type == Type.AI;
    
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new PlayerMoveState(this),
            new PlayerDamageTakenState(this),
            new SkillState(this));
    }
    
    protected override void OnSelected()
    {
        base.OnSelected();
        
        if (GpManager.MainCharacter == this && !GpManager.IsTutorialLevel)
        {
            ShowMoveRange();
        }
    }
}