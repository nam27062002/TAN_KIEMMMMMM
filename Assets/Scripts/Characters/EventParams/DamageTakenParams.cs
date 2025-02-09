using System;
using System.Collections.Generic;

public class DamageTakenParams : StateParams
{
    public int Damage;
    public int ReducedMana;
    public Dictionary<EffectType, int> Effects = new();
    public Action<Character> OnSetDamageTakenFinished;
}