public class DoanGiaLinh : PlayerCharacter
{
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new PlayerDamageTakenState(this),
            new DoanGiaLinh_SkillState(this));
    }
    
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            CharacterInfo.Speed = 11;
        }
        else
        {
            base.SetSpeed();
        }
        
        CharacterInfo.Speed = 60;
    }  
}