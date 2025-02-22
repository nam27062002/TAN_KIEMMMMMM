public class PhamCuChich : PlayerCharacter
{
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new PlayerDamageTakenState(this),
            new PhamCuChich_SkillState(this));
    }
    
    protected override void SetSpeed()
    {
        base.SetSpeed();
        Info.Speed = 200;
    }   
}