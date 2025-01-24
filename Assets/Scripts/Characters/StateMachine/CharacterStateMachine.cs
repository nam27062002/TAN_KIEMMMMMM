using System.Collections.Generic;

public class CharacterStateMachine : StateMachine
{
    public Character Character {get; set;}
    private IdleState IdleState {get; set;}
    private MoveRight MoveRight {get; set;}
    private MoveLeft MoveLeft {get; set;}
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
        MoveRight = new MoveRight(character);
        MoveLeft = new MoveLeft(character);
        DamageTaken = new DamageTaken(character);
        Skill1 = new Skill1(character);
        Skill2 = new Skill2(character);
        Skill3 = new Skill3(character);
        Skill4 = new Skill4(character);
        
        CharacterStates[ECharacterState.Idle] = IdleState;
        CharacterStates[ECharacterState.MoveLeft] = MoveLeft;
        CharacterStates[ECharacterState.MoveRight] = MoveRight;
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
    None,
    Idle,
    MoveRight,
    MoveLeft,
    DamageTaken,
    Skill1,
    Skill2,
    Skill3,
    Skill4
}