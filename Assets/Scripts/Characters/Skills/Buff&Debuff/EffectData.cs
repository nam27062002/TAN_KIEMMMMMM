public class EffectData
{
    public EffectType EffectType;
    public int Duration;
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
}