﻿using UnityEngine;

public class Roll
{
    private readonly CharacterInfo _characterInfo;
    private readonly string _characterName;
    private readonly CharacterAttributes _attributes;
    
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
        Debug.Log($"Base Damage = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetCurrentDamage() / 4} = {baseDamage}");
        return baseDamage;
    }

    public HitChangeParams GetHitChange()
    {
        var rollData = _attributes.hitChangeRollData; 
        var hitChange = RollDice(rollData.rollValue, _characterInfo.GetCurrentDamage() / 2);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] | Hit Change = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetCurrentDamage() / 2} = {hitChange}");
        return new HitChangeParams(){HitChangeValue = hitChange, IsCritical = rollData.rollValue == hitChange};
    }

    public int GetEffectResistance()
    {
        var rollData = _attributes.effectResistanceRollData; 
        var effectResistance = RollDice(rollData, _characterInfo.GetChiDef() / 4);
        if (_characterInfo.Character.GetSkillTurnType() == SkillTurnType.EnemyTurn && _characterInfo.Character == GameplayManager.Instance.SelectedCharacter)
        {
            effectResistance += 5;
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] | Kháng hiệu ứng = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetChiDef() / 4} + 5 = {effectResistance}");
        }
        else
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] | Kháng hiệu ứng = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetChiDef() / 4} = {effectResistance}");
        }
        return effectResistance;
    }

    public int GetEffectCleanse()
    {
        var rollData = _attributes.effectEffectCleanseRollData; 
        var effectCleanse = RollDice(rollData, _characterInfo.GetChiDef() / 4);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] | Giải hiệu ứng = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetChiDef() / 4} = {effectCleanse}");
        return effectCleanse;
    }
    
    public bool IsCritical(int value)
    {
        return _attributes.hitChangeRollData.rollValue == value;
    }
    
    public int GetDodge()
    {
        return _characterInfo.GetDef() + _attributes.spd / 2;
    }
    
    public int GetSpeed()
    {
        return RollDice(12, _attributes.spd / 2);
    }

    private int RollDice(RollData rollData, int add)
    {
        return RollDice(rollData.rollTime, rollData.rollValue, add);   
    }
    
    private static int RollDice(int sides, int add)
    {
        var res = Random.Range(1, sides + 1);
        res += add;
        return res;
    }
    
    public static int RollDice(RollData rollData)
    {
        return RollDice(rollData.rollTime, rollData.rollValue, rollData.add);   
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