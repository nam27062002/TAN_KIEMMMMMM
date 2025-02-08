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
}