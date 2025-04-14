using System.Collections.Generic;
using UnityEngine;

public class AIMoveState : MoveState
{
    public AIMoveState(Character self) : base(self)
    {
    }
    
    protected override void OnReachToTarget(Cell from, Cell to)
    {
        if (from.mainBlockProjectile == null && to.mainBlockProjectile != null)
        {
            Debug.Log($"[{Self.characterConfig.characterName}] đi vào chiến trường => nhận hiệu ứng chảy máu");
            Info.ApplyEffects(new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.Bleed,
                    duration = EffectConfig.DebuffRound,
                }
            });
        }
        base.OnReachToTarget(from, to);
        CoroutineDispatcher.Invoke(HandlePlay, 1f);
    }

    private void HandlePlay()
    {
        ((AICharacter)Self).HandleAIPlay();
    }
}