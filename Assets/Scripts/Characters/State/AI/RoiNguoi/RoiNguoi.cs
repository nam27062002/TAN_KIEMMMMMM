using System;
using System.Collections.Generic;
using UnityEngine;

public class RoiNguoi : AICharacter
{
    public override void SetMainCharacter()
    {
        base.SetMainCharacter();
        PassiveSkill();
    }
    
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new AIMoveState(this),
            new RoiNguoiDamageTakenState(this),
            new ThietNhan_SkillState(this));
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleDeath();
        }
    }
#endif
    private void PassiveSkill()
    {
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}]: passive skill");
        var rollData = Roll.RollDice(1, 20, 0);
        if (rollData >= 10)
        {
            Debug.Log($"Roll data = {rollData} >= 10 => kích hoạt nội tại");
            Info.ApplyEffects(new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    effectType = EffectType.IncreaseDef,
                    actor = this,
                    value = 1,
                    duration = EffectConfig.MAX_ROUND
                },
            });
            foreach (var item in GpManager.Enemies)
            {
                if (item != null && item != this)
                {
                    item.Info.ApplyEffects(new List<EffectData>()
                    {
                        new ChangeStatEffect()
                        {
                            effectType = EffectType.IncreaseDamage,
                            actor = this,
                            value = 2,
                            duration = EffectConfig.MAX_ROUND
                        },
                    });
                }
            }
        }
        else
        {
            Debug.Log($"Roll data = {rollData} < 10 => không kích hoạt nội tại");
        }
    }
    
    public override void HandleAIPlay()
    {
        AlkawaDebug.Log(ELogCategory.AI,"HandleAIPlay");
        GameplayManager.Instance.HandleEndTurn("Không thể di chuyển + dùng skill");
    }
}