using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class SkillInfo
{
    [HorizontalGroup("Skill Info", Width = 64)] 
    [PreviewField(64)] 
    public Sprite icon;

    [HorizontalGroup("Skill Info")]
    [VerticalGroup("Skill Info/Details")] 
    [LabelWidth(75)] 
    public string name;

    [HorizontalGroup("Skill Info")]
    [VerticalGroup("Skill Info/Details")] 
    [LabelWidth(75), ReadOnly] 
    public int skillIndex;
    
    [VerticalGroup("Skill Info/Details")]
    [LabelWidth(75)] 
    [GUIColor(0.2f, 0.6f, 1f)]
    public int mpCost;

    [VerticalGroup("Skill Info/Details")]
    [LabelWidth(75)]
    public int range;

    [VerticalGroup("Skill Info/Details")]
    [LabelWidth(125)]
    [ToggleLeft] 
    public bool isDirectionalSkill;
    
    public DamageTargetType damageType;
    [SerializeField] private EffectIndex effectIndex;
    public BuffType buffType;
    public bool hasApplyDamage;
    [ShowIf(nameof(hasApplyDamage))] public RollData damageConfig;
    public string description;
    [Button("Set Effect")]
    public void SetEffect()
    {
        buffType = effectIndex switch
        {
            EffectIndex.None => BuffType.None,
            
            EffectIndex.E_3_LifeSteal => // Life Steal 
                BuffType.LifeSteal,
            
            EffectIndex.E_5_Sleep => //: Sleep
                BuffType.Sleep,
            
            EffectIndex.E_7_DecreaseStats => //: giảm def, atk vật lý, accuracy, magic def
                BuffType.None,
            
            EffectIndex.E_14_Stun => // 14: Choáng
                BuffType.Stun,
            
            EffectIndex.E_15_Immobilize => // 15: không thể di chuyển
                BuffType.CannotMove,
            
            EffectIndex.E_16_DecreaseResources => // 16: giảm move range, AP
                BuffType.ReduceActionPoints | BuffType.ReduceMoveRange,
            
            EffectIndex.E_17_IncreaseResources => // 17: tăng move range, AP
                BuffType.IncreaseActionPoints | BuffType.IncreaseMoveRange,
            
            EffectIndex.E_21_Block => // 21: chặn sát thương, kỹ năng hoặc đòn đánh
                BuffType.Parry,
            
            EffectIndex.E_31_Blind => // 31: chặn crit, giảm tầm đánh
                BuffType.CannotCrit | BuffType.ReduceAttackSize,
            
            _ => buffType
        };
    }
}

