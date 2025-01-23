using UnityEngine;

public class Roll
{
    private readonly CharacterAttributes _attributes;

    public Roll(CharacterAttributes attributes)
    {
        _attributes = attributes;
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
        return Random.Range(1, sides + 1) + add;
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