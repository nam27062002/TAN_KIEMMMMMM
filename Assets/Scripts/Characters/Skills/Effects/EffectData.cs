using System;
using Newtonsoft.Json;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
[Serializable]
public class EffectData
{
    public EffectType effectType;
    public int duration;
    [NonSerialized] public Character Actor;
    public int characterId;
}