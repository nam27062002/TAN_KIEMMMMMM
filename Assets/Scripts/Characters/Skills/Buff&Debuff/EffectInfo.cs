using System.Collections.Generic;

public class EffectInfo
{
    public List<EffectData> Effects { get; } = new();

    public static readonly HashSet<EffectType> TriggerAtStart = new()
    {
        EffectType.BlockSkill,
        
    };

    public static HashSet<EffectType> TriggerAtEnd = new()
    {
        EffectType.IncreaseDamage,
        EffectType.IncreaseActionPoints,
        EffectType.IncreaseMoveRange
    };

    public void AddEffect(EffectData effect)
    {
        Effects.Add(effect);
    }
}