public class EffectData
{
    public EffectType effectType;
    public int duration;
}

public enum EffectType
{
    None = 0,
    Increase = 1,
    BlockSkill = 2,
}