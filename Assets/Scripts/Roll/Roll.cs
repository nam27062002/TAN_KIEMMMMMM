using UnityEngine;
using System.Linq;

public class Roll
{
    private readonly CharacterInfo _characterInfo;
    private readonly string _characterName;
    private readonly CharacterAttributes _attributes;
    private static bool _isCriticalHit = false;

    public Roll(CharacterInfo characterInfo, string characterName)
    {
        _characterInfo = characterInfo;
        _attributes = _characterInfo.Attributes;
        _characterName = characterName;
    }

    public static void SetCriticalHit(bool isCritical)
    {
        bool oldValue = _isCriticalHit;
        _isCriticalHit = isCritical;

        if (_isCriticalHit && !oldValue)
        {
            Debug.Log("CRITICAL HIT! Added extra dice to all damage sources.");
        }
    }

    public int GetBaseDamage()
    {
        var rollData = _attributes.baseDamageRollData;
        var actualRollTimes = GetActualRollTimes(rollData.rollTime);
        var baseDamage = RollDice(rollData, _characterInfo.GetCurrentDamage() / 4, _isCriticalHit);
        Debug.Log($"Base Damage = {actualRollTimes}d{rollData.rollValue} + {_characterInfo.GetCurrentDamage() / 4} = {baseDamage}");
        return baseDamage;
    }

    public HitChangeParams GetHitChange()
    {
        var rollData = _attributes.hitChangeRollData;
        
        // Kiểm tra tất cả hiệu ứng ReduceHitChange và tính tổng giá trị
        int totalReduceValue = 0;
        var reduceHitChangeEffects = _characterInfo.EffectInfo.Effects
            .Where(e => e.effectType == EffectType.ReduceHitChange)
            .Cast<ChangeStatEffect>();
        
        if (reduceHitChangeEffects.Any())
        {
            foreach (var effect in reduceHitChangeEffects)
            {
                totalReduceValue += effect.value;
            }
            AlkawaDebug.Log(ELogCategory.EFFECT, 
                $"[{_characterName}] has {reduceHitChangeEffects.Count()} increased crit chance effect(s): total -" + totalReduceValue);
        }
        
        // Tính toán hitChange với rollValue đã được điều chỉnh
        int adjustedRollValue = Mathf.Max(2, rollData.rollValue - totalReduceValue); // Đảm bảo rollValue không nhỏ hơn 2
        var hitChange = RollDice(adjustedRollValue, _characterInfo.GetCurrentDamage() / 2);
        
        AlkawaDebug.Log(ELogCategory.SKILL, 
            $"[{_characterName}] | Hit Change = {rollData.rollTime}d{adjustedRollValue} + {_characterInfo.GetCurrentDamage() / 2} = {hitChange}");
        
        // Chí mạng xảy ra khi giá trị xúc xắc bằng với giá trị mặt xúc xắc tối đa sau khi điều chỉnh
        return new HitChangeParams() { 
            HitChangeValue = hitChange, 
            IsCritical = adjustedRollValue == hitChange - _characterInfo.GetCurrentDamage() / 2
        };
    }

    public int GetEffectResistance()
    {
        var rollData = _attributes.effectResistanceRollData;
        var effectResistance = RollDice(rollData, _characterInfo.GetChiDef() / 4);
        if (_characterInfo.Character.GetSkillTurnType() == SkillTurnType.EnemyTurn && _characterInfo.Character == GameplayManager.Instance.SelectedCharacter)
        {
            effectResistance += 5;
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] | Effect Resistance = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetChiDef() / 4} + 5 = {effectResistance}");
        }
        else
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] | Effect Resistance = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetChiDef() / 4} = {effectResistance}");
        }
        return effectResistance;
    }

    public int GetEffectCleanse()
    {
        var rollData = _attributes.effectEffectCleanseRollData;
        var effectCleanse = RollDice(rollData, _characterInfo.GetChiDef() / 4);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{_characterName}] | Effect Cleanse = {rollData.rollTime}d{rollData.rollValue} + {_characterInfo.GetChiDef() / 4} = {effectCleanse}");
        return effectCleanse;
    }
    
    public int GetDodge()
    {
        return _characterInfo.GetDef() + _attributes.spd / 2;
    }

    public int GetSpeed()
    {
        return RollDice(12, _attributes.spd / 2);
    }

    private int RollDice(RollData rollData, int add, bool isCritical = false)
    {
        return RollDice(rollData.rollTime + (isCritical ? 1 : 0), rollData.rollValue, add);
    }

    private static int RollDice(int sides, int add)
    {
        var res = Random.Range(1, sides + 1);
        res += add;
        return res;
    }

    public static int RollDice(RollData rollData, bool isCritical = false)
    {
        return RollDice(rollData.rollTime + (isCritical ? 1 : 0), rollData.rollValue, rollData.add);
    }

    public static int RollDice(int times, int side, int add, bool isCritical = false)
    {
        if (_isCriticalHit || isCritical)
        {
            times += 1;
        }
        var res = add;
        for (int i = 0; i < times; i++)
        {
            res += RollDice(side, 0);
        }
        return res;
    }

    public static int GetActualRollTimes(int baseTimes, bool isCritical = false)
    {
        return baseTimes + ((_isCriticalHit || isCritical) ? 1 : 0);
    }

    public static string GetRollFormula(RollData rollData, int add, bool isCritical = false)
    {
        int actualTimes = GetActualRollTimes(rollData.rollTime, isCritical);
        return $"{actualTimes}d{rollData.rollValue} + {add}";
    }

    public static string GetRollFormula(int times, int side, int add, bool isCritical = false)
    {
        int actualTimes = GetActualRollTimes(times, isCritical);
        return $"{actualTimes}d{side} + {add}";
    }
}