using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine : StateMachine
{
    public Character Character {get; set;}
    private IdleState IdleState {get; set;}
    private MoveState MoveState {get; set;}
    private DamageTakenState DamageTakenState {get; set;}
    private SkillState SkillState {get; set;}

    private Dictionary<ECharacterState, CharacterState> CharacterStates = new();
    
    public CharacterStateMachine(Character character, IdleState idleState, MoveState moveState, DamageTakenState damageTakenState, SkillState skillState)
    {
        Character = character;

        IdleState = idleState;
        MoveState = moveState;
        DamageTakenState = damageTakenState;
        SkillState = skillState;
        
        CharacterStates[ECharacterState.Idle] = IdleState;
        CharacterStates[ECharacterState.Move] = MoveState;
        CharacterStates[ECharacterState.DamageTaken] = DamageTakenState;
        CharacterStates[ECharacterState.Skill] = SkillState;
    }

    public void ChangeState(ECharacterState newState)
    {
        ChangeState(CharacterStates[newState]);
    }
    
    
#if USE_DEBUG
    protected override void ShowDebug(IState newState)
    {
        if (!_canShowDebug) return;
        if (CurrentState != null)
        {
            //AlkawaDebug.Log($"[Gameplay] - {Character.characterConfig.characterName}: [{CurrentState?.NameState}] => [{newState?.NameState}]");
        }
    }
#endif
}

public enum ECharacterState
{
    None = 0,
    Idle = 1,
    Move = 2,
    DamageTaken = 3,
    Skill = 4,
}