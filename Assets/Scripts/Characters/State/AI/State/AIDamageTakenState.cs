using System.Collections.Generic;
using UnityEngine;

public class AIDamageTakenState : DamageTakenState
{
    private CastSkillData _castSkillData = null;
    
    public AIDamageTakenState(Character character) : base(character)
    {
    }
 
    protected override bool CanCounter()
    {
        var target = DamageTakenParams.ReceiveFromCharacter;
        if (target == null) return false;
        if (Info.MustEndTurn) return true;

        List<CastSkillData> castSkillData = Character.GetValidSkills(target);
        if (castSkillData == null || castSkillData.Count == 0) return false;
        _castSkillData = castSkillData[Random.Range(0, castSkillData.Count)];
        return true;
    }
    
    protected override void HandleCounter()
    {
        SetFacing(DamageTakenParams.ReceiveFromCharacter);
        DamageTakenParams.CastSkillData = _castSkillData;
        GpManager.SetCharacterReact(Character, DamageTakenParams);
        HandleCastSkill();
    }

    private void HandleCastSkill()
    {
        List<Character> targets = new List<Character>();
        for (int i = 0; i < _castSkillData.MaxCharactersImpact; i++)
        {
            targets.Add(_castSkillData.CharactersImpact[i]);
        }
        AlkawaDebug.Log(ELogCategory.SKILL,$"[{Character.characterConfig.characterName}] - Counter {DamageTakenParams.ReceiveFromCharacter.characterConfig.characterName}");
        Character.HandleCastSkill(targets, _castSkillData.SkillInfo);
    }
}