using System;
using UnityEngine;

public class EmpressesBowstrings : PassiveSkill
{
    [Header("Tăng Damage & Giảm Crit")]
    // [SerializeField] private int atkIncreasePerRound = 2;
    // [SerializeField] private int maxAtkIncrease = 6;
    // [SerializeField] private int critReductionPerRound = 1;
    // [SerializeField] private int maxCritReduction = 3;
    //
    // [SerializeField] private int selfDamageMin = 1;
    // [SerializeField] private int selfDamageMaxExclusive = 5;
    
    [Header("Giảm Phạm Vi Di Chuyển")]
    [SerializeField] private int initialMoveRange = 8;
    [SerializeField] private int minMoveRange = 3;
    [SerializeField] private int moveRangeDecreasePerRound = 1;
    
    [Header("Chỉ số Né Tránh")]
    [SerializeField] private int initialDodgeBonus = 4;
    
    private int _currentAtkIncrease = 0;
    private int _currentCritReduction = 0;
    private int _currentMoveRange;
    private int _currentDodgeBonus;
    
    private void Start()
    {
        _currentMoveRange = initialMoveRange;
        // if (character.Info != null) 
        //     character.Info.MoveAmount = _currentMoveRange;
        _currentDodgeBonus = initialDodgeBonus;
        IncreaseDodge(_currentDodgeBonus);
    }
    
    public override void RegisterEvents()
    {
        base.RegisterEvents();
        GameplayManager.Instance.OnNewRound += OnNewRound;
    }
    
    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
        if (character.Info != null)
        {
            GameplayManager.Instance.OnNewRound -= OnNewRound;
        }
    }
    
    private void OnNewRound(object sender, EventArgs e)
    {
        if (GameplayManager.Instance.CurrentRound == 0) return;
        // if (_currentAtkIncrease < maxAtkIncrease)
        // {
        //     int increaseAmount = Mathf.Min(atkIncreasePerRound, maxAtkIncrease - _currentAtkIncrease);
        //     _currentAtkIncrease += increaseAmount;
        //     IncreaseDamage(increaseAmount);
        // }
        //
        // if (_currentCritReduction < maxCritReduction)
        // {
        //     int reductionAmount = Mathf.Min(critReductionPerRound, maxCritReduction - _currentCritReduction);
        //     _currentCritReduction += reductionAmount;
        //     ReduceCriticalIndex(reductionAmount);
        // }
        
        if (_currentMoveRange > minMoveRange)
        {
            int decreaseAmount = Mathf.Min(moveRangeDecreasePerRound, _currentMoveRange - minMoveRange);
            _currentMoveRange -= decreaseAmount;
            ReduceMoveRange(decreaseAmount);
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] Hỏa Vũ: giảm phạm vi di chuyển xuống {_currentMoveRange} ô");
        }
        
        if (_currentDodgeBonus > 0)
        {
            ReduceDodge(1);
            _currentDodgeBonus -= 1;
        }
    }
    
    // private void IncreaseDamage(int damage)
    // {
    //     character.Info.Attributes.atk += damage;
    //     AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] Tâm pháp: tăng {damage} damage ({_currentAtkIncrease}/{maxAtkIncrease})");
    // }
    //
    // private void ReduceCriticalIndex(int value)
    // {
    //     character.characterConfig.characterAttributes.hitChangeRollData.rollValue -= value;
    //     AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] Tâm pháp: giảm chỉ số crit ({_currentCritReduction}/{maxCritReduction})");
    // }
    
    private void ReduceMoveRange(int value)
    {
        character.Info.Attributes.maxMoveRange -= value;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] Hỏa Vũ: giảm ô di chuyển (move range = {character.Info.Attributes.maxMoveRange})");

    }
    
    // public bool IsMaxAtkBonus()
    // {
    //     return _currentAtkIncrease >= maxAtkIncrease;
    // }
    
    private void IncreaseDodge(int value)
    {
        if (character.Info == null) return;
        character.Info.Attributes.def += value;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] Né tránh: +{value} def (ban đầu: {character.Info.Attributes.def})");
    }
    
    private void ReduceDodge(int value)
    {
        character.Info.Attributes.def -= value;
        character.Info.Attributes.def = Mathf.Max(0, character.Info.Attributes.def);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] Né tránh: giảm {value} def sau mỗi vòng | def = {character.Info.Attributes.def}");
    }
}
