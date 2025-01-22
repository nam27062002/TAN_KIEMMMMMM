using System;
using Sirenix.OdinInspector;

[Serializable]
public class CharacterInfo
{
    [ShowInInspector, ReadOnly] public Cell Cell { get; set; }
    [ShowInInspector, ReadOnly] public int Speed { get; set; }  
    [ShowInInspector, ReadOnly] public bool LockSkill {get; set;}
    [ShowInInspector, ReadOnly] public CharacterAttributes Attributes { get; set; }
    
    [ShowInInspector, ReadOnly] public int CurrentHP { get; set; }
    [ShowInInspector, ReadOnly] public int CurrentMP { get; set; }
    
    [ShowInInspector, ReadOnly] public int MoveAmount { get; set; }
    [ShowInInspector, ReadOnly] public int MoveBuff { get; set; } 
    // Action
    
    public Action OnHpChanged;
    public Action OnMpChanged;
    
    public SkillConfig SkillConfig { get; set; }
    public Character Character { get; set; }
    public void HandleHpChanged(int value)
    {
        CurrentHP += value;
        OnHpChanged?.Invoke();
    }

    public void HandleMpChanged(int value)
    {
        CurrentMP += value;
        OnMpChanged?.Invoke();
    }
    
    public int GetMoveRange()
    {
        return Attributes.maxMoveRange + MoveBuff - MoveAmount;
    }

    public void ResetBuffAfter()
    {
        
    }
    
    public void ResetBuffBefore()
    {
        MoveAmount = 0;
    }

    public SkillInfo GetSkillInfo(int index, SkillType skillType)
    {
        return SkillConfig.SkillConfigs[skillType][index];
    }

    public void OnCastSkill(SkillInfo skillInfo, Action onEndAnim)
    {
        HandleMpChanged(-skillInfo.mpCost);
        Character.PlaySkillAnim(skillInfo, onEndAnim);
    }

    public void OnDamageTaken(int damage, Action onEndAnim)
    {
        HandleHpChanged(-damage);
        Character.PlayAnim(AnimationParameterNameType.OnDamageTaken, onEndAnim);
    }
}