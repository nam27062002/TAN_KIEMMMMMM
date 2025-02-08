using UnityEngine;

public class Roll
{
    private readonly CharacterAttributes _attributes;
    private string _characterName;
    
    public Roll(CharacterAttributes attributes, string characterName)
    {
        _attributes = attributes;
        _characterName = characterName;
    }
    
    public int GetBaseDamage()
    {
        var rollData = _attributes.baseDamageRollData; 
        var baseDamage = RollDice(rollData, _attributes.atk / 4);
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{_characterName}] Base Damage = {rollData.rollTime}d{rollData.rollValue} + {_attributes.atk / 4} = {baseDamage}");
        return baseDamage;
    }

    public HitChangeParams GetHitChange()
    {
        var rollData = _attributes.hitChangeRollData; 
        var hitChange = RollDice(_attributes.rollValue, _attributes.atk / 2);
        AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{_characterName}] Hit Change = {rollData.rollTime}d{rollData.rollValue} + {_attributes.atk / 2} = {hitChange}");
        return new HitChangeParams(){HitChangeValue = hitChange, IsCritical = rollData.rollValue == hitChange};
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

    public int RollDice(RollData rollData, int add)
    {
        return RollDice(rollData.rollTime, rollData.rollValue, add);   
    }
    
    private static int RollDice(int sides, int add)
    {
        var res = Random.Range(1, sides + 1);
        res += add;
        return res;
    }
    
    private int RollDice(int times, int side, int add)
    {
        var res = add;
        for (int i = 0; i < times; i++)
        {
            res += RollDice(side, 0);
        }
        return res;
    }
}