using System.Collections.Generic;

public class EffectInfo
{
    public List<EffectData> Effects { get; } = new();

    public static readonly HashSet<EffectType> TriggerAtStart = new()
    {
        EffectType.BlockSkill,
        
    };

    public static readonly HashSet<EffectType> TriggerAtEnd = new()
    {
        EffectType.IncreaseDamage,
        EffectType.IncreaseActionPoints,
        EffectType.IncreaseMoveRange,
        EffectType.ReduceMoveRange,
        EffectType.Immobilize,
        EffectType.Stun,
        EffectType.Sleep,
    };
    
    public static readonly Dictionary<EffectType, (int, int)> AppliedEffect = new ()
    {
        { EffectType.Immobilize , (15, 15)},
        { EffectType.Stun, (10, 10)},
        { EffectType.Sleep, (10, 10)},
    };

    public void AddEffect(EffectData effect)
    {
        Effects.Add(effect);
    }
}

public enum EffectType
{
    None = 0,
    IncreaseDamage = 1,
    BlockSkill = 2,
    IncreaseMoveRange = 3,
    IncreaseActionPoints = 4,
    BloodSealEffect = 5,
    BreakBloodSealDamage = 6,
    Immobilize = 7,
    ReduceMoveRange = 8,
    Stun = 9,
    Sleep = 10,
    PoisonPowder = 11,
    RedDahlia = 12,
    Marigold = 13,
    WhiteLotus = 14,
    NightCactus = 15,
    ReduceChiDef = 16,
    RemoveAllPoisonPowder = 17,
}