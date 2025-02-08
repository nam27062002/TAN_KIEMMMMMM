using System;

public class DamageTakenParams : StateParams
{
    public int Damage;
    public int ReducedMana;
    public Action<Character> OnSetDamageTakenFinished;
}