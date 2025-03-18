using System.Collections.Generic;
using UnityEngine;

public class CanSat_SkillState : AISkillState
{
    public CanSat_SkillState(Character character) : base(character)
    {
    }

    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    effectType = EffectType.IncreaseDef,
                    Actor = Character,
                    value = 4,
                    duration = EffectConfig.MAX_ROUND
                },
                new ChangeStatEffect()
                {
                    effectType = EffectType.IncreaseSpd,
                    Actor = Character,
                    value = 6,
                    duration = EffectConfig.MAX_ROUND
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        return new DamageTakenParams()
        {
            Effects = new List<EffectData>()
            {
                new ChangeStatEffect()
                {
                    effectType = EffectType.IncreaseDamage,
                    Actor = Character,
                    value = 2,
                    duration = EffectConfig.MAX_ROUND
                },
                new ChangeStatEffect()
                {
                    effectType = EffectType.ReduceHitChange,
                    Actor = Character,
                    value = 6,
                    duration = EffectConfig.MAX_ROUND
                }
            }
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character)
    {
        var baseDamage = GetBaseDamage();
        var skillDamage = Roll.RollDice(2, 12, 4);
        Debug.Log($"Skill damage = 2d12 +4 = {skillDamage}");
        var realDamage = GetTotalDamage(baseDamage, skillDamage);
        var path = GpManager.MapManager.FindShortestPath(Character.Info.Cell, character.Info.Cell);
        if (path.Count > 2)
        {
            var cells = GpManager.MapManager.GetCellsWalkableInRange(Info.Cell, 1, DirectionType.All);
            character.TeleportToCell(cells[0]);
        }
        HandleTakeAP(character);
        return new DamageTakenParams()
        {
            Damage = realDamage,
            ReducedMana = Utils.RoundNumber(realDamage * 1f / 3),
            Effects = new List<EffectData>()
            {
                new RollEffectData()
                {
                    effectType = EffectType.Poison,
                    Actor = Character,
                    duration = EffectConfig.DebuffRound,
                    rollData = new RollData(1, 6, 0),
                }
            }
        };
    }

    protected override void SetTargetCharacters_Skill3_MyTurn()
    {
        // var allCharacter = new HashSet<Character>(TargetCharacters);
        // foreach (var item in allCharacter)
        // {
        //     AddTargetCharacters(item);
        // }
    }

    protected void HandleTakeAP(Character character)
    {
        var data = Roll.RollDice(1, 20, 0);
        Debug.Log($"Cướp AP: value = 1d20 = {data}");
        if (data < 10)
        {
            character.Info.ApplyEffects(new List<EffectData>()
            {
                new()
                {
                    duration = 2,
                    Actor = Character,
                    effectType = EffectType.CanSat_TakeAP,
                }
            });

            Character.Info.ApplyEffects(new List<EffectData>()
            {
                new ActionPointEffect()
                {
                    duration = 2,
                    Actor = Character,
                    effectType = EffectType.IncreaseActionPoints,
                    actionPoints = new List<int>(3),
                }
            });
        }
    }
}