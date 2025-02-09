using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillState : CharacterState
{
    public SkillState(Character character) : base(character)
    {
        _damageParamsHandlers = new Dictionary<(SkillTurnType, SkillIndex), Func<DamageTakenParams>>
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

    public override string NameState { get; set; } = "Skill";
    private readonly Dictionary<(SkillTurnType, SkillIndex), Func<DamageTakenParams>> _damageParamsHandlers;
    private readonly Dictionary<(SkillTurnType, SkillIndex), Action> _targetCharacterActions;
    protected SkillStateParams SkillStateParams;
    protected readonly List<Character> TargetCharacters = new();
    private bool _waitForFeedback = false;

    public override void OnEnter(StateParams stateParams = null)
    {
        GpManager.SetInteract(false);
        TargetCharacters.Clear();
        SkillStateParams = (SkillStateParams)stateParams;
        if (stateParams is not SkillStateParams skillStateParams) return;
        SkillStateParams = skillStateParams;
        SetTargetCharacters();
        base.OnEnter(stateParams);
        HandleCastSkill();
    }

    private void SetTargetCharacters()
    {
        if (SkillStateParams.Targets is { Count: > 0 })
        {
            foreach (var item in SkillStateParams.Targets)
            {
                TargetCharacters.Add(item);
            }
        }
        if (CanSetTargetCharactersInternal())
        {
            var key = (SkillStateParams.SkillTurnType, SkillStateParams.SkillInfo.skillIndex);
        
            if (_targetCharacterActions.TryGetValue(key, out var action))
            {
                action.Invoke();
            }
        }
    }

    private bool CanSetTargetCharactersInternal()
    {
        return SkillStateParams.SkillInfo.canOverrideSetTargetCharacters || SkillStateParams.Targets == null ||
               SkillStateParams.Targets.Count == 0;
    }

    protected void AddTargetCharacters(Character character)
    {
        TargetCharacters.Add(character);
        SkillStateParams.Targets.Add(character);
    }

    private void HandleCastSkill()
    {
        var animName = GetAnimByIndex(SkillStateParams.SkillInfo.skillIndex);
        PlayAnim(animName, OnCastSkillFinished);
        AlkawaDebug.Log(ELogCategory.CHARACTER,
            $"{Character.characterConfig.characterName} cast skill: {SkillStateParams.SkillInfo.name}");
    }

    protected virtual void OnCastSkillFinished()
    {
        HandleDamageLogic();
        GameplayManager.Instance.UpdateCharacterInfo();
    }
    
    //=======================================================================

    protected virtual DamageTakenParams GetDamageParams()
    {
        var key = (SkillStateParams.SkillTurnType, SkillStateParams.SkillInfo.skillIndex);
        var damageParams = _damageParamsHandlers.TryGetValue(key, out var handler)
            ? handler()
            : new DamageTakenParams();
        return new DamageTakenParams()
        {
            Damage = damageParams.Damage,
            ReducedMana = damageParams.ReducedMana,
            Effects = damageParams.Effects,
            OnSetDamageTakenFinished = HandleTargetFinish,
        };
    }
    
    protected virtual int GetBaseDamage()
    {
        var baseDamage = Info.BaseDamage;
        return baseDamage;
    }

    private HitChangeParams GetHitChangeParams()
    {
        var hitChange = Info.HitChangeParams;
        return hitChange;
    }

    private void HandleDamageLogic()
    {
        HandleDodgeDamage();
    }

    private void HandleDodgeDamage()
    {
        foreach (var target in SkillStateParams.Targets)
        {
            if (Character.Type != target.Type)
            {
                var hitChangeParams = GetHitChangeParams();
                var dodge = target.CharacterInfo.Dodge;
                AlkawaDebug.Log(ELogCategory.CONSOLE,
                    $"[{Character.characterConfig.characterName}] - HitChange = {hitChangeParams.HitChangeValue} | [{target.characterConfig.characterName}] Dodge = {dodge}");
                
                if (hitChangeParams.HitChangeValue <= dodge)
                {
                    CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, new DamageTakenParams{CanDodge = true}));
                    _waitForFeedback = true;
                }
                else
                {
                    CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, GetDamageParams()));
                    _waitForFeedback = true;
                }
            }
            else // Buff cho bản thân hoặc đồng đội
            {
                CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, GetDamageParams()));
            }
        }
    }
    
    private IEnumerator HandleApplyDamage(Character target, DamageTakenParams damageTakenParams)
    {
        if (target != Character)
        {
            yield return new WaitUntil(() => !_waitForFeedback);
            yield return new WaitForSecondsRealtime(0.1f);
            target.OnDamageTaken(damageTakenParams);
        }
        else // self
        {
            Info.OnDamageTaken(damageTakenParams);
            HandleTargetFinish(target);
        }
    }

    private void HandleTargetFinish(Character character)
    {
        TargetCharacters.Remove(character);
        _waitForFeedback = false;
        if (TargetCharacters.Count != 0) return;
        HandleAllTargetFinish();
    }

    protected virtual void HandleAllTargetFinish()
    {
        GpManager.SetInteract(true);
        Character.ChangeState(ECharacterState.Idle);
    }

    #region Skill

    //=====================SKILL 1=====================================
    protected virtual DamageTakenParams GetDamageParams_Skill1_MyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    protected virtual DamageTakenParams GetDamageParams_Skill1_TeammateTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    protected virtual DamageTakenParams GetDamageParams_Skill1_EnemyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    //=====================SKILL 2=====================================
    protected virtual DamageTakenParams GetDamageParams_Skill2_MyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    protected virtual DamageTakenParams GetDamageParams_Skill2_TeammateTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    protected virtual DamageTakenParams GetDamageParams_Skill2_EnemyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    //=====================SKILL 3=====================================
    protected virtual DamageTakenParams GetDamageParams_Skill3_MyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    protected virtual DamageTakenParams GetDamageParams_Skill3_TeammateTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    protected virtual DamageTakenParams GetDamageParams_Skill3_EnemyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    //=====================SKILL 4=====================================
    protected virtual DamageTakenParams GetDamageParams_Skill4_MyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    protected virtual DamageTakenParams GetDamageParams_Skill4_TeammateTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }

    protected virtual DamageTakenParams GetDamageParams_Skill4_EnemyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }
    //=====================SKILL 2=====================================
    protected virtual DamageTakenParams GetDamageParams_PassiveSkill2_MyTurn()
    {
        return new DamageTakenParams
        {
            Damage = GetBaseDamage()
        };
    }
    #endregion

    #region Set Target

    //=====================SKILL 1=====================================
    protected virtual void SetTargetCharacters_Skill1_MyTurn()
    {
        
    }

    protected virtual void SetTargetCharacters_Skill1_TeammateTurn()
    {
        
    }

    protected virtual void SetTargetCharacters_Skill1_EnemyTurn()
    {
        
    }

    //=====================SKILL 2=====================================
    protected virtual void SetTargetCharacters_Skill2_MyTurn()
    {
        
    }

    protected virtual void SetTargetCharacters_Skill2_TeammateTurn()
    {
        
    }

    protected virtual void SetTargetCharacters_Skill2_EnemyTurn()
    {
        
    }

    //=====================SKILL 3=====================================
    protected virtual void SetTargetCharacters_Skill3_MyTurn()
    {
        
    }

    protected virtual void SetTargetCharacters_Skill3_TeammateTurn()
    {
        
    }

    protected virtual void SetTargetCharacters_Skill3_EnemyTurn()
    {
        
    }

    //=====================SKILL 4=====================================
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
    
    private static AnimationParameterNameType GetAnimByIndex(SkillIndex index)
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
            _ => AnimationParameterNameType.None
        };
    }
}