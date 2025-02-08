using System.Collections.Generic;

public class EffectInfo
{
    public List<EffectData> Effects { get; } = new();

    public void AddEffect(EffectData effect)
    {
        Effects.Add(effect);
    }
}