using System;
using System.Collections.Generic;
using System.Linq;
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
    [ShowInInspector, ReadOnly] public List<int> ActionPoints { get; set; } = new(){ 3, 3, 3};
    public List<Cell> MoveCells = new List<Cell>();
    // Action
    
    public Action OnHpChanged;
    public Action OnMpChanged;
    
    public SkillConfig SkillConfig { get; set; }
    public Character Character { get; set; }
    public int Dodge =>  Character.Roll.GetDodge();
    public int BaseDamage => Character.Roll.GetBaseDamage(Character.characterConfig.damageRollData);
    public void HandleHpChanged(int value)
    {
        CurrentHP += value;
        Character.ChangeState(ECharacterState.DamageTaken);
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
        Character.CastSkill(skillInfo, onEndAnim);
    }
    
    public bool CanCastSkill(SkillInfo skillInfo, SkillType skillType)
    {
        return Attributes.mana >= skillInfo.mpCost && IsEnoughActionPoints(skillType);
    }
    
    public bool IsEnoughActionPoints(SkillType skillType)
    {
        var actionPoints = GetActionPoints(skillType);
        return ActionPoints.Any(point => actionPoints < point);
    }
    
    private int GetActionPoints(SkillType skillType)
    {
        return Character.characterConfig.actionPoints[skillType];
    }

    public void OnDamageTaken(int damage, Action onEndAnim)
    {
        string message = "";
        message = damage == 0 ? "Né" : damage.ToString();
        HandleHpChanged(-damage);
        Character.OnEndAnim = onEndAnim;
        Character.hpBar.ShowDamageReceive(message);
    }
}