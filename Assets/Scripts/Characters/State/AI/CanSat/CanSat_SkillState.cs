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
                    EffectType = EffectType.IncreaseDef,
                    Actor = Character,
                    Value = 4,
                    Duration = EffectConfig.MAX_ROUND
                },
                new ChangeStatEffect()
                {
                    EffectType = EffectType.IncreaseSpd,
                    Actor = Character,
                    Value = 6,
                    Duration = EffectConfig.MAX_ROUND
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
                    EffectType = EffectType.IncreaseDamage,
                    Actor = Character,
                    Value = 2,
                    Duration = EffectConfig.MAX_ROUND
                },
                new ChangeStatEffect()
                {
                    EffectType = EffectType.ReduceHitChange,
                    Actor = Character,
                    Value = 6,
                    Duration = EffectConfig.MAX_ROUND
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
        var cells = GpManager.MapManager.GetCellsWalkableInRange(Info.Cell, 1, DirectionType.All);
        character.TeleportToCell(cells[0]);
        HandleTakeAP(character);
        return new DamageTakenParams()
        {
            Damage = realDamage,
            ReducedMana = Utils.RoundNumber(realDamage * 1f / 3),
            Effects = new List<EffectData>()
            {
                new PoisonEffectData()
                {
                    EffectType = EffectType.Poison,
                    Actor = Character,
                    Duration = EffectConfig.DebuffRound,
                    Damage = new RollData(1, 6, 0),
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
                    Duration = 2,
                    Actor = Character,
                    EffectType = EffectType.CanSat_TakeAP,
                }
            });

            Character.Info.ApplyEffects(new List<EffectData>()
            {
                new ActionPointEffect()
                {
                    Duration = 2,
                    Actor = Character,
                    EffectType = EffectType.IncreaseActionPoints,
                    ActionPoints = new List<int>(3),
                }
            });
        }
    }
}