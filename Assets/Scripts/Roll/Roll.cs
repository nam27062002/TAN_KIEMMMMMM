using UnityEngine;

public class Roll
{
    private readonly CharacterAttributes _attributes;

    public Roll(CharacterAttributes attributes)
    {
        _attributes = attributes;
    }
    
    public int GetBaseDamage(RollData rollData)
    {
        return RollDice(rollData.rollTime, rollData.rollValue, _attributes.atk / 4);
    }

    public int GetHitChange()
    {
        return RollDice(_attributes.rollValue, _attributes.atk / 2);
    }

    public bool IsCritical(int value)
    {
        return _attributes.rollValue == value;
    }
    
    public int GetDodge()
    {
        return _attributes.def + _attributes.spd / 2;
    }
    
    public int GetSpeed()
    {
        return RollDice(12, _attributes.spd / 2);
    }

    public int RollDice(RollData rollData)
    {
        return RollDice(rollData.rollTime, rollData.rollValue, rollData.add);   
    }
    
    private static int RollDice(int sides, int add)
    {
        var res = Random.Range(1, sides + 1) + add;
        return res;
    }
    
    private int RollDice(int times, int side, int add)
    {
        var res = add;
        for (int i = 0; i < times; i++)
        {
            res = RollDice(side, 0);
        }
        return res;
    }
}