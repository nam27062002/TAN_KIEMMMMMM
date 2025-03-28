using System.Collections.Generic;
using System.Linq;
public class DoanGiaLinh_DamageTaken : PlayerDamageTakenState
{
    public DoanGiaLinh_DamageTaken(Character character) : base(character)
    {
    }
    
    protected override void SetDamageTakenFinished()
    {
        if (DamageTakenParams.ReceiveFromCharacter.Type != Character.Type)
        {
            var poisonEffects = GetPoisonEffects();
            if (poisonEffects.Count > 0)
            {
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] - Phi Điểu: nhiễm độc cho {DamageTakenParams.ReceiveFromCharacter.characterConfig.characterName}");
                DamageTakenParams.ReceiveFromCharacter.Info.ApplyEffects(poisonEffects);
            }
        }
        base.SetDamageTakenFinished();
    }

    private List<EffectData> GetPoisonEffects()
    {
        var matchingEffect = Info.EffectInfo.Effects
            .OfType<PoisonousBloodPoolEffect>()
            .LastOrDefault(e => e.impacts.Contains(Info.Cell));
        return matchingEffect?.effects ?? new List<EffectData>();
    }
}