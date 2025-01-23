using System.Collections.Generic;

public class CharacterStateMachine : StateMachine
{
    public Character Character {get; set;}
    
    private IdleState IdleState {get; set;}
    private MoveRight MoveRight {get; set;}
    private MoveLeft MoveLeft {get; set;}
    private DamageTaken DamageTaken {get; set;}

    private Dictionary<ECharacterState, CharacterState> CharacterStates = new();
    
    public CharacterStateMachine(Character character)
    {
        Character = character;
        
        IdleState = new IdleState(character);
        MoveRight = new MoveRight(character);
        MoveLeft = new MoveLeft(character);
        DamageTaken = new DamageTaken(character);
        
        CharacterStates[ECharacterState.Idle] = IdleState;
        CharacterStates[ECharacterState.MoveLeft] = MoveLeft;
        CharacterStates[ECharacterState.MoveRight] = MoveRight;
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
    MoveRight,
    MoveLeft,
    DamageTaken,
}