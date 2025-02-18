using System;
using System.Collections.Generic;

public class DamageTakenParams : StateParams
{
    public bool CanDodge;
    public int Damage;
    public int ReducedMana;
    public List<EffectData> Effects = new();
    public Action<FinishApplySkillParams> OnSetDamageTakenFinished;
    public Character ReceiveFromCharacter;
    public bool CanCounter = true;
    public bool WaitCounter;
    public SkillStateParams SkillStateParams;
}