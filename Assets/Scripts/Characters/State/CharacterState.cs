using System;
using UnityEngine;

public abstract class CharacterState : IState
{
    public abstract string NameState { get; set; }
    
    protected Character Character { get; set; }
    protected GameObject Owner => Character.gameObject;
    protected GameObject Model => Character.model;
    protected Transform Transform => Owner.transform;
    protected Vector3 Position => Owner.transform.position;
    protected GameplayManager GpManager => GameplayManager.Instance;
    protected CharacterInfo Info => Character.Info;
    
    protected string CharName => Character.characterConfig.characterName;

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
    
    public void SetFacing()
    {
        var facing = GpManager.GetFacingType(Character);
        SetFacing(facing);
    }
    
    public void SetFacing(Character target)
    {
        var facing = GpManager.GetFacingType(Character, target);
        SetFacing(facing);
    }
    
    public void SetFacing(FacingType facing)
    {
        Model.transform.localScale = facing == FacingType.Right ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"{Character.characterConfig.characterName} set facing to {facing}");
    }
    
    protected void SetCell(Cell cell)
    {
        Character.SetCell(cell);
    }
    
    public void PlayAnim(AnimationParameterNameType animationParameterNameType, Action onEndAnim = null)
    {
        onEndAnim ??= SetIdle;
        Character.AnimationData?.PlayAnimation(animationParameterNameType, onEndAnim);
    }

    #region Sub

    public void SetCharacterPosition()
    {
        var pos = Info.Cell.transform.position;
        pos.y += Character.characterConfig.characterHeight / 2f;
        Transform.position = pos;
    }

    public void SetIdle()
    {
        PlayAnim(AnimationParameterNameType.Idle);
        SetCharacterPosition();
        SetFacing();
    }

    #endregion
}