using System.Collections.Generic;

public class CharacterStateMachine : StateMachine
{
    private Character Character {get; set;}
    private IdleState IdleState {get; set;}
    private MoveState MoveState {get; set;}
    private DamageTakenState DamageTakenState {get; set;}
    private SkillState SkillState {get; set;}

    private readonly Dictionary<ECharacterState, CharacterState> _characterStates = new();
    
    public CharacterStateMachine(Character character, IdleState idleState, MoveState moveState, DamageTakenState damageTakenState, SkillState skillState)
    {
        Character = character;

        IdleState = idleState;
        MoveState = moveState;
        DamageTakenState = damageTakenState;
        SkillState = skillState;
        
        _characterStates[ECharacterState.Idle] = IdleState;
        _characterStates[ECharacterState.Move] = MoveState;
        _characterStates[ECharacterState.DamageTaken] = DamageTakenState;
        _characterStates[ECharacterState.Skill] = SkillState;
    }

    public void ChangeState(ECharacterState newState, StateParams stateParams = null)
    {
        ChangeState(_characterStates[newState], stateParams);
    }

    protected override void ChangeStateMessage(IState newState)
    {
        if (CurrentState != null) 
            AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"{Character.characterConfig.characterName}: [{CurrentState?.NameState}] => [{newState?.NameState}]");
    }
}

public enum ECharacterState
{
    None = 0,
    Idle = 1,
    Move = 2,
    DamageTaken = 3,
    Skill = 4,
}