public class HoacLienHuong_SkillState : SkillState
{
    public HoacLienHuong_SkillState(Character character) : base(character)
    {
    }
    
    protected override void HandleCastSkill()
    {
        base.HandleCastSkill();
        if (_skillStateParams.SkillInfo.skillIndex == SkillIndex.ActiveSkill2)
        {
            MoveToCell(_skillStateParams.TargetCell, 0.5f);
        }
    }
    
    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        return new DamageTakenParams { Damage = GetBaseDamage() };   
    }
        
}