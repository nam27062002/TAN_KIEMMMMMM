﻿using UnityEngine;

public abstract class CharacterState : IState
{
    public abstract string NameState { get; set; }
    
    protected Character Character { get; set; }
    protected GameObject Owner => Character.gameObject;
    protected GameObject Model => Character.model;
    protected Transform Transform => Owner.transform;
    protected Vector3 Position => Owner.transform.position;
    protected GameplayManager GpManager => GameplayManager.Instance;

    protected CharacterState(Character character)
    {
        Character = character;
    }

    public virtual void OnEnter(StateParams stateParams = null){}

    public virtual void OnExit(){}
    
    protected void ReleaseFacing()
    {
        Model.transform.localScale = Vector3.one;
    }
    
    protected void SetFacing()
    {
        var facing = GpManager.GetFacingType(Character);
        SetFacing(facing);
    }
    
    private void SetFacing(FacingType facing)
    {
        Model.transform.localScale = facing == FacingType.Right ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"{Character.characterConfig.characterName} set facing to {facing}");
    }
    
    protected void SetCell(Cell cell)
    {
        Character.SetCell(cell);
    }

    protected virtual void OnFinishAction(CharacterState state)
    {
        Character.ChangeState(Character.PreviousState);
        switch (state)
        {
            case MoveState:
                OnReachToTarget();
                break;
            case SkillState:
                OnCastSkillFinished();
                break;
            case DamageTakenState:
                SetDamageTakenFinished();
                break;
        }
    }

    protected virtual void OnReachToTarget()
    {
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"{Character.characterConfig.characterName} OnReachToTarget - {NameState}");
    }

    protected virtual void OnCastSkillFinished()
    { 
        GpManager.OnCastSkillFinished();
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"{Character.characterConfig.characterName} OnCastSkillFinished - {NameState}");
    }

    protected virtual void SetDamageTakenFinished()
    {
        GpManager.SetDamageTakenFinished();
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"{Character.characterConfig.characterName} SetDamageTakenFinished - {NameState}");
    }
    
    protected virtual void OnFinishAction()
    {
        OnFinishAction(this);
    }
    
}