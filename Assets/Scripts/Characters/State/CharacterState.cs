using UnityEngine;

public abstract class CharacterState : IState
{
    public abstract string NameState { get; set; }
    
    protected Character Character { get; set; }
    protected GameObject Owner => Character.gameObject;
    protected GameObject Model => Character.model;
    protected Transform Transform => Owner.transform;
    protected Vector3 Position => Owner.transform.position;
    protected CharacterManager CharacterManager => Character.CharacterManager;

    protected CharacterState(Character character)
    {
        Character = character;
    }

    public virtual void OnEnter(){}

    public virtual void OnExit(){}
    
    protected void ReleaseFacing()
    {
        Model.transform.localScale = Vector3.one;
    }
    
    protected void SetFacing()
    {
        var facing = CharacterManager.GetFacingType(Character);
        SetFacing(facing);
    }
    
    private void SetFacing(FacingType facing)
    {
        Model.transform.localScale = facing == FacingType.Right ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
    }

    protected virtual void OnFinishAction()
    {
        // Character.OnEndAnimAction();
        // Character.OnEndAnim?.Invoke();
    }
    
}