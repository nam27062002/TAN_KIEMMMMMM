using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanSat : AICharacter
{
    [SerializeField] private GameObject dancerPrefab;
    [SerializeField] private GameObject assassinPrefab;

    public Dancer dancer;

    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new AIMoveState(this),
            new DamageTakenState(this),
            new CanSat_SkillState(this));
    }
    
    protected override void SetIdle(IdleStateParams idleStateParams = null)
    {
        
    }
    
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            Info.Speed = 11;
        }
        else
        {
            base.SetSpeed();
        }
        
        Info.Speed = 1000;
    }
    
    public override void HandleAIPlay()
    {
        AlkawaDebug.Log(ELogCategory.AI, "HandleAIPlay");
        Info.GetMoveRange();
        if (dancer == null && Info.ActionPointsList.Count(p => p == 3) >= 2)
        {
            var cells = GpManager.MapManager.GetCellsWalkableInRange(Info.Cell, 6, DirectionType.All);
            if (cells != null && cells.Count > 0)
            {
                int randomIndex = Random.Range(0, cells.Count);
                var selectedCell = cells[randomIndex];
                var go = Instantiate(dancerPrefab);
                dancer = go.GetComponent<Dancer>();
                dancer.Initialize(selectedCell);
                var skillInfos = GetSkillInfos(SkillTurnType.MyTurn);
                if (skillInfos != null && skillInfos.Count > 1)
                {
                    HandleCastSkill(skillInfos[1], new List<Character> { this });
                    Info.ReduceActionPoints();
                }
            }
        }
        else
        {
            GpManager.HandleEndTurn();
        }
    }
}