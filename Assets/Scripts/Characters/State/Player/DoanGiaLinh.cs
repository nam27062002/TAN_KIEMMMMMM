public class DoanGiaLinh : PlayerCharacter
{
    public TheAllPoisonScript theAllPoisonScript;
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new DoanGiaLinh_DamageTaken(this),
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
        Info.Speed = 1000;
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

    public void SetToggleState(bool state)
    {
        Info.IsToggleOn = state;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}] Toggle venomous parasite: {(state ? "ON" : "OFF")}");
    }
}