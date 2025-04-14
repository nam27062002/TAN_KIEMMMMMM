using System.Collections.Generic;
using System.Linq;
public class DoanGiaLinh_DamageTaken : PlayerDamageTakenState
{
    public DoanGiaLinh_DamageTaken(Character character) : base(character)
    {
    }
    
    protected override void SetDamageTakenFinished()
    {
        // Kiểm tra xem có bị tấn công bởi kẻ địch không
        if (DamageTakenParams.ReceiveFromCharacter.Type != Character.Type)
        {
            // Kiểm tra xem cell hiện tại có phải là vũng máu độc không
            if (Info.Cell.poisonousBloodPool.enabled)
            {
                // Tìm hiệu ứng vũng máu tương ứng do Đoàn Gia Linh tạo ra
                var bloodPoolEffect = FindRelevantBloodPoolEffect();

                if (bloodPoolEffect != null)
                {
                    var poisonEffects = GetEffectsBasedOnFlower(bloodPoolEffect.sourceFlowerType);
                    if (poisonEffects.Count > 0)
                    {
                        AlkawaDebug.Log(ELogCategory.SKILL, 
                            $"[{CharName}] - Phi Điểu ({bloodPoolEffect.sourceFlowerType}): nhiễm độc cho {DamageTakenParams.ReceiveFromCharacter.characterConfig.characterName}");
                        DamageTakenParams.ReceiveFromCharacter.Info.ApplyEffects(poisonEffects);
                    }
                }
            }
        }
        base.SetDamageTakenFinished();
    }

    // Tìm hiệu ứng PoisonousBloodPool do chính nhân vật này tạo ra và đang ảnh hưởng đến cell hiện tại
    private PoisonousBloodPoolEffect FindRelevantBloodPoolEffect()
    {
        // Duyệt qua tất cả hiệu ứng của tất cả nhân vật để tìm hiệu ứng vũng máu phù hợp
        foreach (var character in GpManager.Characters)
        {
            var effect = character.Info.EffectInfo.Effects
                .OfType<PoisonousBloodPoolEffect>()
                .FirstOrDefault(e => e.Actor == Character && e.impacts.Contains(Info.Cell)); // Tìm hiệu ứng do chính mình tạo và ảnh hưởng đến cell này
                
            if (effect != null)
            {
                return effect;
            }
        }
        return null; // Không tìm thấy
    }

    // Lấy danh sách hiệu ứng độc dựa trên loại hoa
    private List<EffectData> GetEffectsBasedOnFlower(EffectType flowerType)
    {
        var effects = new List<EffectData>();
        switch (flowerType)
        {
            case EffectType.RedDahlia: // Thược Dược Đỏ => Fear + Gắn hoa đỏ
                effects.Add(new EffectData { effectType = EffectType.Fear, duration = EffectConfig.DebuffRound, Actor = Character });
                effects.Add(new EffectData { effectType = EffectType.RedDahlia, duration = EffectConfig.DebuffRound, Actor = Character }); // Gắn lại hoa
                break;
            case EffectType.WhiteLotus: // Sen Trắng => Sleep + Gắn hoa trắng
                effects.Add(new EffectData { effectType = EffectType.Sleep, duration = EffectConfig.DebuffRound, Actor = Character });
                effects.Add(new EffectData { effectType = EffectType.WhiteLotus, duration = EffectConfig.DebuffRound, Actor = Character }); // Gắn lại hoa
                break;
            case EffectType.Marigold: // Cúc Vạn Thọ => Sleep + Stun + Gắn hoa vàng
                effects.Add(new EffectData { effectType = EffectType.Sleep, duration = EffectConfig.DebuffRound, Actor = Character });
                effects.Add(new EffectData { effectType = EffectType.Stun, duration = EffectConfig.DebuffRound, Actor = Character });
                effects.Add(new EffectData { effectType = EffectType.Marigold, duration = EffectConfig.DebuffRound, Actor = Character }); // Gắn lại hoa
                break;
            case EffectType.NightCactus: // Xương Rồng Đêm => Immobilize + Poison + Gắn hoa xương rồng
                effects.Add(new EffectData { effectType = EffectType.Immobilize, duration = EffectConfig.DebuffRound, Actor = Character });
                effects.Add(new RollEffectData
                {
                    effectType = EffectType.Poison,
                    duration = EffectConfig.DebuffRound,
                    rollData = new RollData(1, 4, 0), 
                    Actor = Character
                });
                effects.Add(new EffectData { effectType = EffectType.NightCactus, duration = EffectConfig.DebuffRound, Actor = Character }); // Gắn lại hoa
                break;
        }
        return effects;
    }
}