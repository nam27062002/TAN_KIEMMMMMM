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
        
        if (Character.lastDamageTakenCountered)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Không thể counter vì đã counter lần trước.");
            return false;
        }

        if (Info.CurrentHp > Character.GetMaxHp() / 2)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Không thể counter: ({Info.CurrentHp}/{Character.GetMaxHp()})");
            return false;
        }

        var target = DamageTakenParams.ReceiveFromCharacter;
        if (target == null)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Không thể counter vì không có mục tiêu.");
            return false;
        }

        if (Info.MustEndTurn)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Có thể counter vì phải kết thúc lượt.");
            return true;
        }

        List<CastSkillData> castSkillData = Character.GetValidSkills(target);
        if (castSkillData == null || castSkillData.Count == 0)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Không thể counter vì không có kỹ năng hợp lệ.");
            return false;
        }

        if (Random.value > 0.3f)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Không thể counter: {Random.value} > 0.3");
            return false;
        }

        _castSkillData = castSkillData[Random.Range(0, castSkillData.Count)];
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - dùng skill{_castSkillData.SkillInfo.name} lên {target.characterConfig.characterName}");

        return true;
    }
    
    protected override void HandleCounter()
    {
        SetFacing(DamageTakenParams.ReceiveFromCharacter);
        DamageTakenParams.CastSkillData = _castSkillData;
        GpManager.SetCharacterReact(Character, DamageTakenParams);
        HandleCastSkill();
        
        Character.lastDamageTakenCountered = true;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{Character.characterConfig.characterName}] - Đã thực hiện counter thành công");
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