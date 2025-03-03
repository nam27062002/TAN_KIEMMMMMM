using System;
using Sirenix.OdinInspector;
[Serializable]
public class CharacterAttributes
{
    public int maxMoveRange;
    public int health;
    public int mana;
    [ShowIf("@!overrideDamage")] public int atk;
    public int def;
    public int chiDef;
    public int spd;
    public RollData baseDamageRollData = new() { rollTime = 1, rollValue = 8 };
    public RollData hitChangeRollData = new() { rollTime = 1, rollValue = 20 };
    public RollData effectResistanceRollData = new() { rollTime = 1, rollValue = 20 };
    public RollData effectEffectCleanseRollData = new() { rollTime = 1, rollValue = 20 };
    
    [Title("Override")]
    public bool overrideDamage;
    [ShowIf("@overrideDamage")] public int atkOverride;
    public bool overrideMaxMoveRange;
    [ShowIf("@overrideMaxMoveRange")] public int maxMoveRangeOverride;
}