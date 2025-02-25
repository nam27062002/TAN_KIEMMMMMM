public class CanSat_SkillState : AISkillState
{
    public CanSat_SkillState(Character character) : base(character)
    {
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        return new DamageTakenParams();
    }
}