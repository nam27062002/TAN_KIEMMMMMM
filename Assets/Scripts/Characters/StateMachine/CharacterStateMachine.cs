using System.Collections.Generic;

public class CharacterStateMachine : StateMachine
{
    public Character Character {get; set;}
    
    private IdleState IdleState {get; set;}
    private MoveState MoveState {get; set;}
    private DamageTaken DamageTaken {get; set;}

    private Dictionary<ECharacterState, CharacterState> CharacterStates = new();
    
    public CharacterStateMachine(Character character)
    {
        Character = character;
        
        IdleState = new IdleState(character);
        MoveState = new MoveState(character);
        DamageTaken = new DamageTaken(character);
        
        CharacterStates[ECharacterState.Idle] = IdleState;
        CharacterStates[ECharacterState.Move] = MoveState;
        CharacterStates[ECharacterState.DamageTaken] = DamageTaken;
    }

    public void ChangeState(ECharacterState newState)
    {
        ChangeState(CharacterStates[newState]);
    }
}

public enum ECharacterState
{
    None,
    Idle,
    Move,
    DamageTaken,
}