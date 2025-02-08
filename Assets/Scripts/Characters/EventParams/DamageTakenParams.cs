using System;

public class DamageTakenParams : StateParams
{
    public int Damage;
    public int ReducedMana;
    public int IncreaseDamage;
    public Action<Character> OnSetDamageTakenFinished;
}