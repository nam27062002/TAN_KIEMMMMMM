using System;
[Serializable]
public class CharacterAttributes
{
    public int maxMoveRange;
    public int health;
    public int mana;
    public int atk;
    public int def;
    public int chiDef;
    public int spd;
    public int rollValue;
    public RollData baseDamageRollData = new() { rollTime = 1, rollValue = 8 };
    public RollData hitChangeRollData = new() { rollTime = 1, rollValue = 20 };
    public RollData effectResistanceRollData = new() { rollTime = 1, rollValue = 20 };
    public RollData effectEffectCleanseRollData = new() { rollTime = 1, rollValue = 20 };

}