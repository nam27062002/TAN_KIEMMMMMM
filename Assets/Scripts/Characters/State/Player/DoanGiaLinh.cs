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
            Info.Speed = 11;
        }
        else
        {
            base.SetSpeed();
        }
#if UNITY_EDITOR
        Info.Speed = 100;
#endif
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