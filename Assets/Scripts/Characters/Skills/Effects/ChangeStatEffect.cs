using System;

[Serializable]
public class ChangeStatEffect : EffectData
{
    public int value;
}

public enum StatType
{
    None = 0,
    Damage = 1,
}