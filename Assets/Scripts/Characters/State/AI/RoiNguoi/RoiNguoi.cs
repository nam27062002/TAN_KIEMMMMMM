using System.Collections.Generic;
using UnityEngine;

public class RoiNguoi : AICharacter
{
    
    public override void SetMainCharacter()
    {
        base.SetMainCharacter();
        PassiveSkill();
    }

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
                    EffectType = EffectType.IncreaseDef,
                    Actor = this,
                    Value = 1,
                    Duration = EffectConfig.MAX_ROUND
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
                            EffectType = EffectType.IncreaseDamage,
                            Actor = this,
                            Value = 2,
                            Duration = EffectConfig.MAX_ROUND
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
        // if (!TryMoving())
        // {
        //     GameplayManager.Instance.HandleEndTurn();
        // }
        GameplayManager.Instance.HandleEndTurn();
    }
}