public class DoanGiaLinh : PlayerCharacter
{
    public TheAllPoisonScript theAllPoisonScript;
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

    public int GetVenomousParasite()
    {
        return theAllPoisonScript.VenomousParasite;
    }

    public void SetVenomousParasite(int venomousParasite)
    {
        theAllPoisonScript.VenomousParasite = venomousParasite;
    }
}