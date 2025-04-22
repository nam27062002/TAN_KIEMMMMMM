using System.Collections.Generic;
using UnityEngine;

public class AIMoveState : MoveState
{
    public AIMoveState(Character character) : base(character)
    {
    }
    
    protected override void OnReachToTarget(Cell from, Cell to)
    {
        if (to.poisonousBloodPool.enabled)
        {
            AlkawaDebug.Log(ELogCategory.EFFECT, 
                $"[{Character.characterConfig.characterName}] entered poisonous blood pool => Blind");
            Info.ApplyEffects(new List<EffectData>()
            {
                new()
                {
                    effectType = EffectType.Blind,
                    duration = EffectConfig.DebuffRound,
                    Actor = Character
                }
            });
        }

        if (from.mainBlockProjectile == null && to.mainBlockProjectile != null)
        {
            Debug.Log($"[{Character.characterConfig.characterName}] entered battlefield => received bleed effect");
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
        ((AICharacter)Character).HandleAIPlay();
    }
}