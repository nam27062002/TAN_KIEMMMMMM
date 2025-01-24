using UnityEngine;

public abstract class CharacterState : IState
{
    public abstract string NameState { get; set; }
    
    protected Character Character { get; set; }

    protected CharacterState(Character character)
    {
        Character = character;
    }

    public virtual void OnEnter(){}

    public virtual void OnExit(){}

    protected virtual void OnEndAnim()
    {
        Character.OnEndAnimAction();
        Character.OnEndAnim?.Invoke();
    }
}