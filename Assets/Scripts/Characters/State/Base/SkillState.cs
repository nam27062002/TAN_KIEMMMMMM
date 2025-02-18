using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SkillState : CharacterState
{
    public override string NameState { get; set; } = "Skill";

    private readonly Dictionary<(SkillTurnType, SkillIndex), Func<Character, DamageTakenParams>> _damageParamsHandlers;
    private readonly Dictionary<(SkillTurnType, SkillIndex), Action> _targetCharacterActions;
    protected SkillStateParams _skillStateParams;
    protected readonly List<Character> TargetCharacters = new();
    private bool _waitForFeedback = false;
    protected bool WaitForReact = false;

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
    }

    public override void OnEnter(StateParams stateParams = null)
    {
        GpManager.SetInteract(false);
        TargetCharacters.Clear();

        if (!(stateParams is SkillStateParams skillStateParams))
        {
            return;
        }

        _skillStateParams = skillStateParams;

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

        if (CanSetTargetCharactersInternal())
        {
            var key = (_skillStateParams.SkillTurnType, _skillStateParams.SkillInfo.skillIndex);
            if (_targetCharacterActions.TryGetValue(key, out var action))
            {
                action.Invoke();
            }
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
            CanCounter = true,
        };
    }

    protected virtual int GetBaseDamage() => Info.BaseDamage;

    private HitChangeParams GetHitChangeParams() => Info.HitChangeParams;

    private void HandleDamageLogic() => HandleDodgeDamage();

    private void HandleDodgeDamage()
    {
        if (_skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                if (Character.Type != target.Type)
                {
                    var hitChangeParams = GetHitChangeParams();
                    var dodge = target.Info.Dodge;
                    AlkawaDebug.Log(ELogCategory.SKILL,
                        $"[{Character.characterConfig.characterName}] - HitChange = {hitChangeParams.HitChangeValue} | " +
                        $"[{target.characterConfig.characterName}] Dodge = {dodge}");

// #if ALWAY_APPLY_EFFECT
//                     if (hitChangeParams.HitChangeValue < dodge && Character.Type == Type.AI)
// #else 
//                     if (hitChangeParams.HitChangeValue < dodge)
// #endif
                    if (hitChangeParams.HitChangeValue < dodge)
                    {
                        var dodgeDamageParams = new DamageTakenParams
                        {
                            CanDodge = true,
                            ReceiveFromCharacter = Character,
                            CanCounter = true,
                            OnSetDamageTakenFinished = HandleTargetFinish,
                        };
                        CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, dodgeDamageParams));
                        _waitForFeedback = true;
                    }
                    else
                    {
                        CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, GetDamageParams(target)));
                        _waitForFeedback = true;
                    }
                }
                else
                {
                    var damageParams = GetDamageParams(target);
                    damageParams.CanCounter = false;
                    CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, damageParams));
                }
            }
        }
        else
        {
            HandleAllTargetFinish();
        }
    }

    private IEnumerator HandleApplyDamage(Character target, DamageTakenParams damageTakenParams)
    {
        if (target != Character)
        {
            yield return new WaitUntil(() => !_waitForFeedback);
            yield return new WaitForSecondsRealtime(0.1f);

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
        GpManager.SetInteract(true);
        Character.ChangeState(ECharacterState.Idle);

        if (_skillStateParams.EndTurnAfterFinish)
        {
            GpManager.HandleEndTurn();
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
    protected virtual void SetTargetCharacters_Skill1_MyTurn() { }

    protected virtual void SetTargetCharacters_Skill1_TeammateTurn() { }

    protected virtual void SetTargetCharacters_Skill1_EnemyTurn() { }

    //===================== SKILL 2 =====================
    protected virtual void SetTargetCharacters_Skill2_MyTurn() { }

    protected virtual void SetTargetCharacters_Skill2_TeammateTurn() { }

    protected virtual void SetTargetCharacters_Skill2_EnemyTurn() { }

    //===================== SKILL 3 =====================
    protected virtual void SetTargetCharacters_Skill3_MyTurn() { }

    protected virtual void SetTargetCharacters_Skill3_TeammateTurn() { }

    protected virtual void SetTargetCharacters_Skill3_EnemyTurn() { }

    //===================== SKILL 4 =====================
    protected virtual void SetTargetCharacters_Skill4_MyTurn() { }

    protected virtual void SetTargetCharacters_Skill4_TeammateTurn() { }

    protected virtual void SetTargetCharacters_Skill4_EnemyTurn() { }

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
        var targetPos = cell.transform.position;
        targetPos.y += Character.characterConfig.characterHeight / 2f;
        Character.UnRegisterCell();
        var moveSequence = DOTween.Sequence();
        moveSequence.Append(Transform.DOMove(targetPos, time).SetEase(Ease.Linear));
        moveSequence.OnComplete(() =>
        {
            Character.SetCell(cell);
        });
    }
}
