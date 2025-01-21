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
    
    // Action
    
    public Action OnHpChanged;
    public Action OnMpChanged;

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
}