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
        EffectType.ReduceChiDef,
        EffectType.Poison,
        EffectType.ThietNhan_Poison,
        EffectType.ThietNhan_ReduceMoveRange,
        EffectType.ThietNhan_BlockAP,
        EffectType.Cover_50_Percent,
        EffectType.Disarm,
        EffectType.Shield,
        EffectType.Taunt,
        EffectType.Cover_PhamCuChich_Skill3,
        EffectType.ReduceAP,
    };
    
    public static readonly Dictionary<EffectType, (int, int)> AppliedEffect = new ()
    {
        { EffectType.Immobilize , (15, 15)},
        { EffectType.Stun, (10, 10)},
        { EffectType.Sleep, (10, 10)},
        { EffectType.ReduceChiDef, (15, 15)},
        { EffectType.Poison, (15, 15)},
        { EffectType.ThietNhan_Poison, (10, 10)},
        { EffectType.ThietNhan_ReduceMoveRange, (10, 10)},
        { EffectType.ThietNhan_BlockAP, (10, 10)},
        { EffectType.Disarm, (10, 10)},
        { EffectType.Taunt, (15, 15)},
        { EffectType.Silence, (10, 10)},
        { EffectType.ReduceAP, (15, 15)}
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
    VenomousParasite = 18,
    Poison = 19,
    ThietNhan_Poison = 20,
    ThietNhan_ReduceMoveRange = 21,
    ThietNhan_BlockAP = 22,
    ThietNhan_Infected = 23,
    Cover_50_Percent = 24,
    Disarm = 25,
    Cover_100_Percent = 26,
    SnakeArmor = 27,
    DragonArmor = 28,
    Shield = 29,
    Taunt = 30,
    Silence = 31,
    Cover_PhamCuChich_Skill3 = 32,
    ReduceStat_PhamCuChich_Skill3 = 34,
    ReduceAP = 35,
}