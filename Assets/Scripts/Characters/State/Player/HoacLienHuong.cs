public class HoacLienHuong : PlayerCharacter
{
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new PlayerDamageTakenState(this),
            new HoacLienHuong_SkillState(this));
    }
    
    protected override void SetSpeed()
    {
        base.SetSpeed();
        Info.Speed = 100;
    }
}