using System.Collections.Generic;

public class DoanGiaLinh_SkillState : SkillState
{
    public DoanGiaLinh_SkillState(Character character) : base(character)
    {
    }
    
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn()
    {
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{Character.characterConfig.characterName}] Nhiên Huyết");
        return new DamageTakenParams
        {
            Effects = new Dictionary<EffectType, int>()
            {
                // { EffectType.BlockSkill , 0},
                // { EffectType.IncreaseActionPoints , 1},
                // { EffectType.IncreaseMoveRange , 2},
            }
        };
    }
}