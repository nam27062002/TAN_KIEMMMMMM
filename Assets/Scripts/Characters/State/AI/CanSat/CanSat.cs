using System.Collections;
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
    
    protected override IEnumerator HandleSpecialAction()
    {
        StartCoroutine(Summer(dancerPrefab, GetSkillInfos(SkillTurnType.MyTurn)[1]));
        yield return new WaitForSeconds(1f);
        StartCoroutine(Summer(assassinPrefab, GetSkillInfos(SkillTurnType.EnemyTurn)[1]));
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
            StartCoroutine(Summer(dancerPrefab, GetSkillInfos(SkillTurnType.MyTurn)[1]));
            Info.ReduceActionPoints();
        }
        else
        {
            GpManager.HandleEndTurn();
        }
    }

    private IEnumerator Summer(GameObject type, SkillInfo skillInfo, bool dontNeedActionPoints = false)
    {
        HandleCastSkill(skillInfo, new List<Character> { this }, dontNeedActionPoints);
        if (!dontNeedActionPoints) Info.ReduceActionPoints();
        yield return new WaitForSeconds(1f);
        SpawnEnemy(type);
    }

    private void SpawnEnemy(GameObject gameObj)
    {
        var cells = GpManager.MapManager.GetCellsWalkableInRange(Info.Cell, 6, DirectionType.All);
        if (cells is not { Count: > 0 }) return;
        int randomIndex = Random.Range(0, cells.Count);
        var selectedCell = cells[randomIndex];
        var go = Instantiate(gameObj);
        dancer = go.GetComponent<Dancer>();
        dancer.Initialize(selectedCell);
        dancer.owner = this;
    }
}