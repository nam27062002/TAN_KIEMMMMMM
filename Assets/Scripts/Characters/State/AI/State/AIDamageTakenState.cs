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
        if (!DamageTakenParams.CanCounter) return false;
        if (GpManager.SelectedCharacter.Type == Type.AI) return false;

        var target = DamageTakenParams.ReceiveFromCharacter;
        if (target == null)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Cannot counter because there is no target.");
            return false;
        }

        if (Info.MustEndTurn)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Cannot counter because must end turn.");
            return false;
        }

        List<CastSkillData> castSkillData = Character.GetValidSkills(target);
        if (castSkillData == null || castSkillData.Count == 0)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Cannot counter because no valid skills.");
            return false;
        }
#if !UNITY_EDITOR
        if (Character.lastDamageTakenCountered)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Cannot counter because already countered last time.");
            return false;
        }

        if (Info.CurrentHp > Character.GetMaxHp() / 2)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Cannot counter: ({Info.CurrentHp}/{Character.GetMaxHp()})");
            return false;
        }

        if (Random.value > 0.3f)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Cannot counter: {Random.value} > 0.3");
            return false;
        }
#endif
        _castSkillData = castSkillData[Random.Range(0, castSkillData.Count)];
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - using skill {_castSkillData.SkillInfo.name} on {target.characterConfig.characterName}");

        return true;
    }
    
    protected override void HandleCounter()
    {
        SetFacing(DamageTakenParams.ReceiveFromCharacter);
        DamageTakenParams.CastSkillData = _castSkillData;
        GpManager.SetCharacterReact(Character, DamageTakenParams);
        HandleCastSkill();
        
        Character.lastDamageTakenCountered = true;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Counter executed successfully");
    }

    private void HandleCastSkill()
    {
        List<Character> targets = new List<Character>();
        for (int i = 0; i < _castSkillData.MaxCharactersImpact; i++)
        {
            targets.Add(_castSkillData.CharactersImpact[i]);
        }
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Counter {DamageTakenParams.ReceiveFromCharacter.characterConfig.characterName}");
        Character.HandleCastSkill(targets, _castSkillData.SkillInfo);
    }
}