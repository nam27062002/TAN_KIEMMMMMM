using System;
using System.Collections.Generic;

public class DamageTakenParams : StateParams
{
    public bool CanDodge;
    public int Damage;
    public int ReducedMana;
    public Dictionary<EffectType, int> Effects = new();
    public Action<FinishApplySkillParams> OnSetDamageTakenFinished;
    public Character ReceiveFromCharacter;
    public bool CanCounter = true;
    public bool WaitCounter;
    
    // params
    public SkillStateParams SkillStateParams;
}