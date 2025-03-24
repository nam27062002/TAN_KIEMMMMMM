using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class SkillState : CharacterState
{
    public override string NameState { get; set; } = "Skill";

    private readonly Dictionary<(SkillTurnType, SkillIndex), Func<Character, DamageTakenParams>> _damageParamsHandlers;
    private readonly Dictionary<(SkillTurnType, SkillIndex), Action> _targetCharacterActions;
    private readonly Dictionary<(SkillTurnType, SkillIndex), Action> _handleAfterDamageTakenFinish;
    protected SkillStateParams _skillStateParams;
    protected readonly List<Character> TargetCharacters = new();
    protected HashSet<Character> mainTargetCharacter = new();
    private bool _waitForFeedback = false;
    protected bool WaitForReact = false;
    protected string SkillName => _skillStateParams.SkillInfo.name;

    public SkillState(Character character) : base(character)
    {
        _damageParamsHandlers = new Dictionary<(SkillTurnType, SkillIndex), Func<Character, DamageTakenParams>>
        {
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill1), GetDamageParams_Skill1_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill2), GetDamageParams_Skill2_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill3), GetDamageParams_Skill3_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill4), GetDamageParams_Skill4_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.PassiveSkill2), GetDamageParams_PassiveSkill2_MyTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill1), GetDamageParams_Skill1_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill2), GetDamageParams_Skill2_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill3), GetDamageParams_Skill3_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill4), GetDamageParams_Skill4_TeammateTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill1), GetDamageParams_Skill1_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill2), GetDamageParams_Skill2_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill3), GetDamageParams_Skill3_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill4), GetDamageParams_Skill4_EnemyTurn }
        };


        _targetCharacterActions = new Dictionary<(SkillTurnType, SkillIndex), Action>
        {
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill1), SetTargetCharacters_Skill1_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill2), SetTargetCharacters_Skill2_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill3), SetTargetCharacters_Skill3_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill4), SetTargetCharacters_Skill4_MyTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill1), SetTargetCharacters_Skill1_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill2), SetTargetCharacters_Skill2_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill3), SetTargetCharacters_Skill3_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill4), SetTargetCharacters_Skill4_TeammateTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill1), SetTargetCharacters_Skill1_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill2), SetTargetCharacters_Skill2_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill3), SetTargetCharacters_Skill3_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill4), SetTargetCharacters_Skill4_EnemyTurn }
        };

        _handleAfterDamageTakenFinish = new Dictionary<(SkillTurnType, SkillIndex), Action>
        {
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill1), HandleAfterDamageTakenFinish_Skill1_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill2), HandleAfterDamageTakenFinish_Skill2_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill3), HandleAfterDamageTakenFinish_Skill3_MyTurn },
            { (SkillTurnType.MyTurn, SkillIndex.ActiveSkill4), HandleAfterDamageTakenFinish_Skill4_MyTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill1), HandleAfterDamageTakenFinish_Skill1_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill2), HandleAfterDamageTakenFinish_Skill2_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill3), HandleAfterDamageTakenFinish_Skill3_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill4), HandleAfterDamageTakenFinish_Skill4_TeammateTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill1), HandleAfterDamageTakenFinish_Skill1_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill2), HandleAfterDamageTakenFinish_Skill2_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill3), HandleAfterDamageTakenFinish_Skill3_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill4), HandleAfterDamageTakenFinish_Skill4_EnemyTurn }
        };
    }

    public override void OnEnter(StateParams stateParams = null)
    {
        GpManager.SetInteract(false);
        TargetCharacters.Clear();

        if (stateParams is not SkillStateParams skillStateParams)
        {
            return;
        }

        _skillStateParams = skillStateParams;
        mainTargetCharacter = new HashSet<Character>(_skillStateParams.Targets);
        if (_skillStateParams.IdleStateParams != null)
        {
            Character.HandleCounterLogic(_skillStateParams);
        }
        else
        {
            SetTargetCharacters();
            base.OnEnter(stateParams);
            HandleCastSkill();
        }
    }

    private void SetTargetCharacters()
    {
        if (_skillStateParams.Targets is { Count: > 0 })
        {
            foreach (var item in _skillStateParams.Targets)
            {
                TargetCharacters.Add(item);
            }
        }

        if (!CanSetTargetCharactersInternal()) return;
        var key = (_skillStateParams.SkillTurnType, _skillStateParams.SkillInfo.skillIndex);
        if (_targetCharacterActions.TryGetValue(key, out var action))
        {
            action.Invoke();
        }
    }

    private void HandleAfterDamageTakenFinish()
    {
        var key = (_skillStateParams.SkillTurnType, _skillStateParams.SkillInfo.skillIndex);
        if (_handleAfterDamageTakenFinish.TryGetValue(key, out var action))
        {
            action.Invoke();
        }
    }

    private bool CanSetTargetCharactersInternal()
    {
        return _skillStateParams.SkillInfo.canOverrideSetTargetCharacters ||
               _skillStateParams.Targets == null ||
               _skillStateParams.Targets.Count == 0;
    }

    protected void AddTargetCharacters(Character character)
    {
        TargetCharacters.Add(character);
        _skillStateParams.Targets.Add(character);
    }

    protected void RemoveAllTargetCharacters()
    {
        TargetCharacters.Clear();
        _skillStateParams.Targets.Clear();
    }

    protected virtual void HandleCastSkill()
    {
        var animName = GetAnimByIndex(_skillStateParams.SkillInfo.skillIndex);
        PlayAnim(animName, OnCastSkillFinished);
        AlkawaDebug.Log(ELogCategory.CHARACTER,
            $"{Character.characterConfig.characterName} cast skill: {_skillStateParams.SkillInfo.name}");
    }

    protected virtual void OnCastSkillFinished()
    {
        HandleDamageLogic();
        GameplayManager.Instance.UpdateCharacterInfo();
    }

    //=======================================================================
    // Damage & Damage Params
    protected virtual DamageTakenParams GetDamageParams(Character character)
    {
        Debug.Log("________________________________________________________________");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] - {SkillName}");
        var key = (_skillStateParams.SkillTurnType, _skillStateParams.SkillInfo.skillIndex);
        var damageParams = _damageParamsHandlers.TryGetValue(key, out var handler)
            ? handler(character)
            : new DamageTakenParams();
        return new DamageTakenParams
        {
            Damage = damageParams.Damage,
            ReducedMana = damageParams.ReducedMana,
            Effects = damageParams.Effects,
            OnSetDamageTakenFinished = HandleTargetFinish,
            ReceiveFromCharacter = Character,
            CanCounter = Character.Type == GpManager.MainCharacter.Type && Character.Type != character.Type,
            SkillStateParams = _skillStateParams
        };
    }

    protected virtual int GetBaseDamage() => Info.BaseDamage;

    private HitChangeParams GetHitChangeParams(Character character)
    {
        if (character.Info.EffectInfo.Effects.Any(p => p.effectType == EffectType.Prone))
        {
            AlkawaDebug.Log(ELogCategory.EFFECT, $"{character.characterConfig.characterName} có hiệu ứng LỢI THẾ");
            var roll1 = Info.HitChangeParams;
            var roll2 = Info.HitChangeParams;
            AlkawaDebug.Log(ELogCategory.EFFECT, $"{character.characterConfig.characterName} roll1 = {roll1.HitChangeValue} | roll2 = {roll2.HitChangeValue}");
            return roll1.HitChangeValue > roll2.HitChangeValue ? roll1 : roll2;
        }

        if (Info.EffectInfo.Effects.Any(p => p.effectType == EffectType.Fear && p.Actor == character))
        {
            AlkawaDebug.Log(ELogCategory.EFFECT, $"{Character.characterConfig.characterName} có hiệu ứng BẤT LỢI");
            var roll1 = Info.HitChangeParams;
            var roll2 = Info.HitChangeParams;
            AlkawaDebug.Log(ELogCategory.EFFECT, $"{character.characterConfig.characterName} roll1 = {roll1.HitChangeValue} | roll2 = {roll2.HitChangeValue}");
            return roll1.HitChangeValue > roll2.HitChangeValue ? roll2 : roll1;
        }
        
        return Info.HitChangeParams;
    }

    private void HandleDamageLogic() => HandleDodgeDamage();

    private void HandleDodgeDamage()
    {
        if (_skillStateParams.Targets.Count == 0)
        {
            HandleAllTargetFinish();
            return;
        }

        foreach (var target in _skillStateParams.Targets)
        {
            if (Character.Type != target.Type)
            {
                ProcessEnemyTarget(target);
            }
            else
            {
                ProcessFriendlyTarget(target);
            }
        }
    }

    private void ProcessEnemyTarget(Character target)
    {
        if (HasValidShield(target))
        {
            target.Info.Cell.mainShieldCell.ReceiveDamage(target, Character);
            HandleTargetFinish(new FinishApplySkillParams()
            {
                Character = target,
                WaitForCounter = false,
            });
        }
        else if (HasBlockProjectile(target))
        {
            HandleTargetFinish(new FinishApplySkillParams()
            {
                Character = target,
                WaitForCounter = false,
            });
            AlkawaDebug.Log(ELogCategory.EFFECT, $"Kim Hà Tại: chặn sát thương cho {target.characterConfig.characterName} từ đòn đánh của {Character.characterConfig.characterName}");
        }
        else
        {
            var hitChangeParams = GetHitChangeParams(target);
            var dodge = target.Info.Dodge;

            AlkawaDebug.Log(ELogCategory.SKILL,
                $"[{Character.characterConfig.characterName}] - HitChange = {hitChangeParams.HitChangeValue} | " +
                $"[{target.characterConfig.characterName}] Dodge = {dodge}");
        
            if (hitChangeParams.HitChangeValue < dodge)
            {
                if (target.Info.EffectInfo.Effects.Any(p => p.effectType == EffectType.Drunk && p is DrunkEffect { SleepWhileMiss: true }))
                {
                    Debug.Log($"{target.characterConfig.characterName} có hiệu ứng say, {CharName} đánh hụt => sleep");
                    Character.Info.ApplyEffect(                
                        new EffectData
                        {
                            effectType = EffectType.Sleep,
                            duration = EffectConfig.DebuffRound,
                            Actor = Character
                        });
                    var damageParams = GetDamageParams(target);
                    Character.Info.HandleDamageTaken(-damageParams.Damage, target);
                    Debug.Log($"{CharName} bị phản sát thương: damage = {damageParams.Damage}");
                }
                var dodgeDamageParams = new DamageTakenParams
                {
                    CanDodge = true,
                    ReceiveFromCharacter = Character,
                    CanCounter = Character.Type == GpManager.MainCharacter.Type && Character.Type != target.Type,
                    OnSetDamageTakenFinished = HandleTargetFinish,
                    SkillStateParams = _skillStateParams
                };
                Character.HandleMpChanged(_skillStateParams.SkillInfo.mpCost); 
                CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, dodgeDamageParams));
            }
            else
            {
                CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, GetDamageParams(target)));
            }

            _waitForFeedback = true;   
        }
    }
    
    private void ProcessFriendlyTarget(Character target)
    {
        var damageParams = GetDamageParams(target);
        damageParams.CanCounter = false;
        CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, damageParams));
    }

    private bool HasValidShield(Character target)
    {
        var shieldCell = target.Info.Cell.mainShieldCell;
        return shieldCell != null && shieldCell != Character.Info.Cell.mainShieldCell;
    }

    private bool HasBlockProjectile(Character target)
    {
        var shieldCell = target.Info.Cell.mainBlockProjectile;
        return shieldCell != null && shieldCell != Character.Info.Cell.mainBlockProjectile;
    }

    protected virtual void HandleApplyDamageOnEnemy(Character character)
    {
    }
    
    private IEnumerator HandleApplyDamage(Character target, DamageTakenParams damageTakenParams)
    {
        if (target != Character)
        {
            yield return new WaitUntil(() => !_waitForFeedback);
            yield return new WaitForSecondsRealtime(0.1f);
            if (target.Type != Character.Type)
            {
                HandleApplyDamageOnEnemy(target);   
            }
            if (!target.Info.IsDie)
            {
                target.OnDamageTaken(damageTakenParams);
            }
            else
            {
                damageTakenParams.OnSetDamageTakenFinished?.Invoke(new FinishApplySkillParams
                {
                    Character = target,
                    WaitForCounter = false,
                });
            }
        }
        else // Xử lý damage cho chính bản thân
        {
            Info.OnDamageTaken(damageTakenParams);
            HandleTargetFinish(new FinishApplySkillParams
            {
                Character = target,
                WaitForCounter = false,
            });
        }
    }

    private void HandleTargetFinish(FinishApplySkillParams applySkillParams)
    {
        TargetCharacters.Remove(applySkillParams.Character);
        _waitForFeedback = false;
        WaitForReact = applySkillParams.WaitForCounter;

        if (TargetCharacters.Count == 0)
        {
            HandleAllTargetFinish();
        }
    }

    protected virtual void HandleAllTargetFinish()
    {
        HandleAfterDamageTakenFinish();
        GpManager.SetInteract(true);
        Character.ChangeState(ECharacterState.Idle);

        if (_skillStateParams.EndTurnAfterFinish)
        {
            GpManager.SetInteract(false);
        }

        AlkawaDebug.Log(ELogCategory.CHARACTER,
            $"{Character.characterConfig.characterName} HandleAllTargetFinish");
    }

    #region Skill Damage Params

    //===================== SKILL 1 =====================
    protected virtual DamageTakenParams GetDamageParams_Skill1_MyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected virtual DamageTakenParams GetDamageParams_Skill1_TeammateTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected virtual DamageTakenParams GetDamageParams_Skill1_EnemyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    //===================== SKILL 2 =====================
    protected virtual DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected virtual DamageTakenParams GetDamageParams_Skill2_TeammateTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected virtual DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    //===================== SKILL 3 =====================
    protected virtual DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected virtual DamageTakenParams GetDamageParams_Skill3_TeammateTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected virtual DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    //===================== SKILL 4 =====================
    protected virtual DamageTakenParams GetDamageParams_Skill4_MyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected virtual DamageTakenParams GetDamageParams_Skill4_TeammateTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    protected virtual DamageTakenParams GetDamageParams_Skill4_EnemyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    //===================== PASSIVE SKILL 2 =====================
    protected virtual DamageTakenParams GetDamageParams_PassiveSkill2_MyTurn(Character character) =>
        new DamageTakenParams { Damage = GetBaseDamage() };

    #endregion

    #region Set Target Characters

    //===================== SKILL 1 =====================
    protected virtual void SetTargetCharacters_Skill1_MyTurn()
    {
    }

    protected virtual void SetTargetCharacters_Skill1_TeammateTurn()
    {
    }

    protected virtual void SetTargetCharacters_Skill1_EnemyTurn()
    {
    }

    //===================== SKILL 2 =====================
    protected virtual void SetTargetCharacters_Skill2_MyTurn()
    {
    }

    protected virtual void SetTargetCharacters_Skill2_TeammateTurn()
    {
    }

    protected virtual void SetTargetCharacters_Skill2_EnemyTurn()
    {
    }

    //===================== SKILL 3 =====================
    protected virtual void SetTargetCharacters_Skill3_MyTurn()
    {
    }

    protected virtual void SetTargetCharacters_Skill3_TeammateTurn()
    {
    }

    protected virtual void SetTargetCharacters_Skill3_EnemyTurn()
    {
    }

    //===================== SKILL 4 =====================
    protected virtual void SetTargetCharacters_Skill4_MyTurn()
    {
    }

    protected virtual void SetTargetCharacters_Skill4_TeammateTurn()
    {
    }

    protected virtual void SetTargetCharacters_Skill4_EnemyTurn()
    {
    }

    #endregion
    
    #region Set Target Characters

    //===================== SKILL 1 =====================
    protected virtual void HandleAfterDamageTakenFinish_Skill1_MyTurn()
    {
    }

    protected virtual void HandleAfterDamageTakenFinish_Skill1_TeammateTurn()
    {
    }

    protected virtual void HandleAfterDamageTakenFinish_Skill1_EnemyTurn()
    {
    }

    //===================== SKILL 2 =====================
    protected virtual void HandleAfterDamageTakenFinish_Skill2_MyTurn()
    {
    }

    protected virtual void HandleAfterDamageTakenFinish_Skill2_TeammateTurn()
    {
    }

    protected virtual void HandleAfterDamageTakenFinish_Skill2_EnemyTurn()
    {
    }

    //===================== SKILL 3 =====================
    protected virtual void HandleAfterDamageTakenFinish_Skill3_MyTurn()
    {
    }

    protected virtual void HandleAfterDamageTakenFinish_Skill3_TeammateTurn()
    {
    }

    protected virtual void HandleAfterDamageTakenFinish_Skill3_EnemyTurn()
    {
    }

    //===================== SKILL 4 =====================
    protected virtual void HandleAfterDamageTakenFinish_Skill4_MyTurn()
    {
    }

    protected virtual void HandleAfterDamageTakenFinish_Skill4_TeammateTurn()
    {
    }

    protected virtual void HandleAfterDamageTakenFinish_Skill4_EnemyTurn()
    {
    }

    #endregion

    protected static AnimationParameterNameType GetAnimByIndex(SkillIndex index)
    {
        return index switch
        {
            SkillIndex.ActiveSkill1 => AnimationParameterNameType.Skill1,
            SkillIndex.ActiveSkill2 => AnimationParameterNameType.Skill2,
            SkillIndex.ActiveSkill3 => AnimationParameterNameType.Skill3,
            SkillIndex.ActiveSkill4 => AnimationParameterNameType.Skill4,
            SkillIndex.PassiveSkill1 => AnimationParameterNameType.Skill1,
            SkillIndex.PassiveSkill2 => AnimationParameterNameType.Skill1,
            SkillIndex.PassiveSkill3 => AnimationParameterNameType.Skill1,
            _ => AnimationParameterNameType.None,
        };
    }

    protected void MoveToCell(Cell cell, float time)
    {
        ReleaseFacing();
        SetFacing(Info.Cell.transform.position.x > cell.transform.position.x ? FacingType.Left : FacingType.Right);
        var targetPos = cell.transform.position;
        targetPos.y += Character.characterConfig.characterHeight / 2f;
        Character.UnRegisterCell();
        var moveSequence = DOTween.Sequence();
        moveSequence.Append(Transform.DOMove(targetPos, time).SetEase(Ease.Linear));
        moveSequence.OnComplete(() =>
        {
            Character.SetCell(cell);
            Info.Cell.ShowFocus();
        });
    }

    protected void TeleportToCell(Cell cell)
    {
        Info.Cell.HideFocus();
        Info.Cell.Character = null;
        Info.Cell.CellType = CellType.Walkable;

        cell.Character = Character;
        cell.CellType = CellType.Character;
        Character.Info.Cell = cell;
        SetCharacterPosition();
        SetFacing();
    }

    protected int GetSkillDamage(RollData rollData)
    {
        var damage = Roll.RollDice(rollData);
        Debug.Log($"Skill Damage = {rollData.rollTime}d{rollData.rollValue} + {rollData.add} = {damage}");
        return damage;
    }

    protected int GetTotalDamage(int baseDamage, int skillDamage)
    {
        var totalDamage = baseDamage + skillDamage;
        Debug.Log($"Total Damage = {baseDamage} + {skillDamage} = {totalDamage}");
        return totalDamage;
    }
}