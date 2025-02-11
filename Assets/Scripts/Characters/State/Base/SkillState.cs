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
    private SkillStateParams _skillStateParams;
    protected readonly List<Character> TargetCharacters = new();
    private bool _waitForFeedback = false;
    protected bool WaitForReact = false;

    public override void OnEnter(StateParams stateParams = null)
    {
        GpManager.SetInteract(false);
        TargetCharacters.Clear();
        _skillStateParams = (SkillStateParams)stateParams;
        if (stateParams is not SkillStateParams skillStateParams) return;
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
        return _skillStateParams.SkillInfo.canOverrideSetTargetCharacters || _skillStateParams.Targets == null ||
               _skillStateParams.Targets.Count == 0;
    }

    protected void AddTargetCharacters(Character character)
    {
        TargetCharacters.Add(character);
        _skillStateParams.Targets.Add(character);
    }

    private void HandleCastSkill()
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

    protected virtual DamageTakenParams GetDamageParams()
    {
        var key = (_skillStateParams.SkillTurnType, _skillStateParams.SkillInfo.skillIndex);
        var damageParams = _damageParamsHandlers.TryGetValue(key, out var handler)
            ? handler()
            : new DamageTakenParams();
        return new DamageTakenParams()
        {
            Damage = damageParams.Damage,
            ReducedMana = damageParams.ReducedMana,
            Effects = damageParams.Effects,
            OnSetDamageTakenFinished = HandleTargetFinish,
            ReceiveFromCharacter = Character,
            CanCounter = true,
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
        if (_skillStateParams.Targets.Count > 0)
        {
            foreach (var target in _skillStateParams.Targets)
            {
                if (Character.Type != target.Type)
                {
                    var hitChangeParams = GetHitChangeParams();
                    var dodge = target.CharacterInfo.Dodge;
                    AlkawaDebug.Log(ELogCategory.CONSOLE,
                        $"[{Character.characterConfig.characterName}] - HitChange = {hitChangeParams.HitChangeValue} | [{target.characterConfig.characterName}] Dodge = {dodge}");
                
                    if (hitChangeParams.HitChangeValue <= dodge)
                    {
                        CoroutineDispatcher.RunCoroutine(HandleApplyDamage(target, 
                            new DamageTakenParams
                            {
                                CanDodge = true,
                                ReceiveFromCharacter = Character,
                                CanCounter = true,
                                OnSetDamageTakenFinished = HandleTargetFinish,
                            }));
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
                    var damageParams = GetDamageParams();
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
            target.OnDamageTaken(damageTakenParams);
        }
        else // self
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
        if (TargetCharacters.Count != 0) return;
        HandleAllTargetFinish();
    }

    protected virtual void HandleAllTargetFinish()
    {
        GpManager.SetInteract(true);
        Character.ChangeState(ECharacterState.Idle);
        if (_skillStateParams.EndTurnAfterFinish)
        {
            GpManager.HandleEndTurn();
        }
        AlkawaDebug.Log(ELogCategory.CHARACTER, $"{Character.characterConfig.characterName} HandleAllTargetFinish");
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