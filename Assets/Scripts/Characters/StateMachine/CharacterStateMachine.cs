using System.Collections.Generic;

public class CharacterStateMachine : StateMachine
{
    public Character Character {get; set;}
    private IdleState IdleState {get; set;}
    private Move Move {get; set;}
    private DamageTaken DamageTaken {get; set;}
    private Skill1 Skill1 {get; set;}
    private Skill2 Skill2 {get; set;}
    private Skill3 Skill3 {get; set;}
    private Skill4 Skill4 {get; set;}

    private Dictionary<ECharacterState, CharacterState> CharacterStates = new();
    
    public CharacterStateMachine(Character character)
    {
        Character = character;
        
        IdleState = new IdleState(character);
        Move = new Move(character);
        DamageTaken = new DamageTaken(character);
        Skill1 = new Skill1(character);
        Skill2 = new Skill2(character);
        Skill3 = new Skill3(character);
        Skill4 = new Skill4(character);
        
        CharacterStates[ECharacterState.Idle] = IdleState;
        CharacterStates[ECharacterState.Move] = Move;
        CharacterStates[ECharacterState.DamageTaken] = DamageTaken;
        CharacterStates[ECharacterState.Skill1] = Skill1;
        CharacterStates[ECharacterState.Skill2] = Skill2;
        CharacterStates[ECharacterState.Skill3] = Skill3;
        CharacterStates[ECharacterState.Skill4] = Skill4;
    }

    public void ChangeState(ECharacterState newState)
    {
        ChangeState(CharacterStates[newState]);
    }
}

public enum ECharacterState
{
    None = 0,
    Idle = 1,
    Move = 2,
    DamageTaken = 3,
    Skill1 = 4,
    Skill2 = 5,
    Skill3 = 6,
    Skill4 = 7
}