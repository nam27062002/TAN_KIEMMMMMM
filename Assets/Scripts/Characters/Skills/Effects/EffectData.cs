using System;
[Serializable]
public class EffectData : IEffectData
{
    public EffectType effectType;
    public int duration;
    [NonSerialized] public Character Actor;
    public int characterId;
}