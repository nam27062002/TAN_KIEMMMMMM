using System;
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
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill1), GetDamageParams_Skill1_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill2), GetDamageParams_Skill2_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill3), GetDamageParams_Skill3_TeammateTurn },
            { (SkillTurnType.TeammateTurn, SkillIndex.ActiveSkill4), GetDamageParams_Skill4_TeammateTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill1), GetDamageParams_Skill1_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill2), GetDamageParams_Skill2_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill3), GetDamageParams_Skill3_EnemyTurn },
            { (SkillTurnType.EnemyTurn, SkillIndex.ActiveSkill4), GetDamageParams_Skill4_EnemyTurn }
        };
    }
    
    public override string NameState { get; set; } = "Skill";
    private readonly Dictionary<(SkillTurnType, SkillIndex), Func<DamageTakenParams>> _damageParamsHandlers;
    private SkillStateParams _skillStateParams;
    private readonly HashSet<Character> _targetCharacters = new();
    
    public override void OnEnter(StateParams stateParams = null)
    {
        _targetCharacters.Clear();
        _skillStateParams = (SkillStateParams)stateParams;
        if (_skillStateParams != null)
        {
            foreach (var item in _skillStateParams.Targets)
            {
                _targetCharacters.Add(item);
            }   
        }
        base.OnEnter(stateParams);
        HandleCastSkill();
    }

    private void HandleCastSkill()
    {
        var animName = GetAnimByIndex(_skillStateParams.SkillInfo.skillIndex);
        PlayAnim(animName, OnFinishAction);
        AlkawaDebug.Log(ELogCategory.CHARACTER, $"{Character.characterConfig.characterName} cast skill: {_skillStateParams.SkillInfo.name}");
    }
    
    protected override void OnCastSkillFinished()
    {
        HandleDamageLogic();
        base.OnCastSkillFinished();
    }
    
    private static AnimationParameterNameType GetAnimByIndex(SkillIndex index)
    {
        return index switch
        {
            SkillIndex.ActiveSkill1 => AnimationParameterNameType.Skill1,
            SkillIndex.ActiveSkill2  => AnimationParameterNameType.Skill2,
            SkillIndex.ActiveSkill3  => AnimationParameterNameType.Skill3,
            SkillIndex.ActiveSkill4  => AnimationParameterNameType.Skill4,
            SkillIndex.PassiveSkill1  => AnimationParameterNameType.Skill1,
            SkillIndex.PassiveSkill2  => AnimationParameterNameType.Skill1,
            SkillIndex.PassiveSkill3  => AnimationParameterNameType.Skill1,
            _ => AnimationParameterNameType.None
        };
    }
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
    
    //=======================================================================
    
    protected virtual DamageTakenParams GetDamageParams()
    {
        var key = (_skillStateParams.SkillTurnType, _skillStateParams.SkillInfo.skillIndex);
        return _damageParamsHandlers.TryGetValue(key, out var handler) ? handler() : new DamageTakenParams();
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
        foreach (var target in _skillStateParams.Targets)
        {
            var hitChangeParams = GetHitChangeParams();
            var dodge = target.CharacterInfo.Dodge;
            AlkawaDebug.Log(ELogCategory.CONSOLE, $"[{Character.characterConfig.characterName}] - HitChange = {hitChangeParams.HitChangeValue} | [{target.characterConfig.characterName}] Dodge = {dodge}");

            if (hitChangeParams.HitChangeValue <= dodge)
            {
                HandleDodgeDamageSuccess(target);
            }
            else
            {
                HandleApplyDamage(target);
            }
        }
    }
    
    private void HandleDodgeDamageSuccess(Character target)
    {
        target.ShowMessage("Né");
        HandleTargetFinish(target);
    }

    private void HandleApplyDamage(Character target)
    {
        target.OnDamageTaken(GetDamageParams());
    }
    
    private void HandleTargetFinish(Character character)
    {
        _targetCharacters.Remove(character);
        if (_targetCharacters.Count == 0)
        {
            Debug.Log("NT - HandleTargetFinish");
        }
    }
}