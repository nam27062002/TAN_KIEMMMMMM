using System;
using UnityEngine;

public abstract class CharacterState : IState
{
    public abstract string NameState { get; set; }
    
    protected Character Self { get; set; }
    protected GameObject Owner => Self.gameObject;
    protected GameObject Model => Self.model;
    protected Transform Transform => Owner.transform;
    protected Vector3 Position => Owner.transform.position;
    protected GameplayManager GpManager => GameplayManager.Instance;
    protected CharacterInfo Info => Self.Info;
    
    protected string CharName => Self.characterConfig.characterName;

    protected CharacterState(Character self)
    {
        Self = self;
    }

    public virtual void OnEnter(StateParams stateParams = null){}

    public virtual void OnExit(){}
    
    protected void ReleaseFacing()
    {
        Model.transform.localScale = Vector3.one;
    }
    
    public void SetFacing()
    {
        var facing = GpManager.GetFacingType(Self);
        SetFacing(facing);
    }
    
    public void SetFacing(Character target)
    {
        var facing = GpManager.GetFacingType(Self, target);
        SetFacing(facing);
    }
    
    public void SetFacing(FacingType facing)
    {
        Model.transform.localScale = facing == FacingType.Right ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"{Self.characterConfig.characterName} set facing to {facing}");
    }
    
    protected void SetCell(Cell cell)
    {
        Self.SetCell(cell);
    }
    
    public void PlayAnim(AnimationParameterNameType animationParameterNameType, Action onEndAnim = null)
    {
        onEndAnim ??= SetIdle;
        Self.AnimationData?.PlayAnimation(animationParameterNameType, onEndAnim);
    }

    #region Sub

    public void SetCharacterPosition()
    {
        if (Self == null || Info.IsDie) return;
        var pos = Info.Cell.transform.position;
        pos.z = pos.y;
        pos.y += Self.characterConfig.characterHeight / 2f;
        pos.z = pos.y;
        Transform.position = pos;
    }

    public void SetIdle()
    {
        if (Self == null || Info.IsDie) return;
        PlayAnim(AnimationParameterNameType.Idle);
        SetCharacterPosition();
        SetFacing();
    }

    #endregion
}