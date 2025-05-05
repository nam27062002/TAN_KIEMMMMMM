using System;
[Serializable]
public class RollEffectData : EffectData
{
    public RollData rollData;
    public bool dontRoll = false;
    public int value = 0;
}