using System.Collections.Generic;

public class CharacterStateMachine : StateMachine
{
    public Character Character {get; set;}
    private IdleState IdleState {get; set;}
    private MoveRight MoveRight {get; set;}
    private MoveLeft MoveLeft {get; set;}
    private DamageTaken DamageTaken {get; set;}
    private Skill1 Skill1 {get; set;}

    private Dictionary<ECharacterState, CharacterState> CharacterStates = new();
    
    public CharacterStateMachine(Character character)
    {
        Character = character;
        
        IdleState = new IdleState(character);
        MoveRight = new MoveRight(character);
        MoveLeft = new MoveLeft(character);
        DamageTaken = new DamageTaken(character);
        Skill1 = new Skill1(character);
        
        CharacterStates[ECharacterState.Idle] = IdleState;
        CharacterStates[ECharacterState.MoveLeft] = MoveLeft;
        CharacterStates[ECharacterState.MoveRight] = MoveRight;
        CharacterStates[ECharacterState.DamageTaken] = DamageTaken;
        CharacterStates[ECharacterState.Skill1] = Skill1;
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
    Skill1,
}