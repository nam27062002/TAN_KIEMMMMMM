﻿using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class BloodyRainbow : PassiveSkill
{
    [BoxGroup("HP Damage Trigger")]
    [SerializeField] private int increasedDamageValue = 1;
    [BoxGroup("HP Damage Trigger")]
    [SerializeField] private int hpDamageThreshold = 4;
    [BoxGroup("HP Damage Trigger")]
    [SerializeField] private int maxDamageTriggers = 5;
    
    [BoxGroup("Crit Trigger")]
    [SerializeField] private int hpCritThreshold = 8;
    [BoxGroup("Crit Trigger")]
    [SerializeField] private int critThresholdReduction = 1;
    [BoxGroup("Crit Trigger")]
    [SerializeField] private int maxCritTriggers = 3;
    
    [BoxGroup("Internal Force Trigger")]
    [SerializeField] private int internalForceThreshold = 2;
    [BoxGroup("Internal Force Trigger")]
    [SerializeField] private int increasedDamageTaken = 2;
    [BoxGroup("Internal Force Trigger")]
    [SerializeField] private int maxInternalForceTriggers = 2;
    
    private int _hpDecreased;
    private int _internalForceDecreased;
    private int _damageTriggerCount;
    private int _critTriggerCount;
    private int _internalForceTriggerCount;
    
    public override void RegisterEvents()
    {
        base.RegisterEvents();
        character.Info.OnHpChanged += OnHpChanged;
        character.Info.OnMpChanged += OnMPChanged;
    }

    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
        if (character.Info != null)
        {
            character.Info.OnHpChanged -= OnHpChanged;
            character.Info.OnMpChanged -= OnMPChanged;
        }
    }

    private void OnHpChanged(object sender, int value)
    {
        if (value < 0)
        {
            _hpDecreased -= value;
            ProcessTrigger(ref _hpDecreased, hpDamageThreshold, maxDamageTriggers, ref _damageTriggerCount, IncreaseDamage);
            ProcessTrigger(ref _hpDecreased, hpCritThreshold, maxCritTriggers, ref _critTriggerCount, ReduceCritRequirement);
        }
    }

    private void OnMPChanged(object sender, int value)
    {
        if (value < 0)
        {
            _internalForceDecreased -= value;
            ProcessTrigger(ref _internalForceDecreased, internalForceThreshold, maxInternalForceTriggers, ref _internalForceTriggerCount, IncreaseDamageTaken);
        }
    }

    private void ProcessTrigger(ref int counter, int threshold, int maxTriggers, ref int currentTriggers, Action effect)
    {
        int possibleTriggers = counter / threshold;
        if (possibleTriggers > currentTriggers)
        {
            int triggersToApply = Mathf.Min(possibleTriggers - currentTriggers, maxTriggers - currentTriggers);
            for (int i = 0; i < triggersToApply; i++)
            {
                effect();
            }
            currentTriggers += triggersToApply;
        }
    }

    private void IncreaseDamage()
    {
        character.Info.Attributes.atk += increasedDamageValue;
        character.ShowMessage("Tăng 1 sát thương");
        AlkawaDebug.Log(ELogCategory.SKILL, $"{character.characterConfig.characterName} tăng {increasedDamageValue} damage");
    }

    private void ReduceCritRequirement()
    {
        // Create a ReduceHitChange effect to permanently reduce crit count by 1
        var reduceHitChangeEffect = new ChangeStatEffect
        {
            effectType = EffectType.ReduceHitChange,
            Actor = character,
            duration = EffectConfig.MAX_ROUND, // Vĩnh viễn (tồn tại trong cả trận đấu)
            value = critThresholdReduction // Giảm số lần crit đi 1
        };

        // Apply the effect to the character
        character.Info.ApplyEffect(reduceHitChangeEffect);
        
        // Hiển thị thông báo
        character.ShowMessage($"Reduce crit requirement by {critThresholdReduction}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] reduced crit requirement by {critThresholdReduction}, increased crit chance");
    }

    private void IncreaseDamageTaken()
    {
        // Tăng sát thương nhận vào
        character.Info.IncreasedDamageTaken += increasedDamageTaken;
        
        //AlkawaDebug.Log(ELogCategory.SKILL, $"{character.characterConfig.characterName} - Phi Hồng: Tăng {increasedMovement} di chuyển và nhận {increasedDamageTaken} sát thương");
    }
}
