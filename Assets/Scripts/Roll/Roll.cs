using UnityEngine;

public class Roll
{
    private readonly CharacterInfo _characterInfo;
    private string _characterName;
    private CharacterAttributes _attributes;
    
    public Roll(CharacterInfo characterInfo, string characterName)
    {
        _characterInfo = characterInfo;
        _attributes = _characterInfo.Attributes;
        _characterName = characterName;
    }
    
    public int GetBaseDamage()
    {
        var rollData = _attributes.baseDamageRollData; 
        var baseDamage = RollDice(rollData, _characterInfo.GetCurrentDamage() / 4);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] Base Damage = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetCurrentDamage() / 4} = {baseDamage}");
        return baseDamage;
    }

    public HitChangeParams GetHitChange()
    {
        var rollData = _attributes.hitChangeRollData; 
        var hitChange = RollDice(_attributes.rollValue, _characterInfo.GetCurrentDamage() / 2);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] Hit Change = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetCurrentDamage() / 2} = {hitChange}");
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
    
    public static int RollDice(int times, int side, int add)
    {
        var res = add;
        for (int i = 0; i < times; i++)
        {
            res += RollDice(side, 0);
        }
        return res;
    }
}