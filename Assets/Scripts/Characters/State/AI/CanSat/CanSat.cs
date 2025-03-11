using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanSat : AICharacter
{
    [SerializeField] private GameObject dancerPrefab;
    [SerializeField] private GameObject assassinPrefab;

    public Dancer dancer;
    public Assassin assassin;

    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new AIMoveState(this),
            new AIDamageTakenState(this),
            new CanSat_SkillState(this));
    }

    protected override IEnumerator HandleSpecialAction()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(Summer(dancerPrefab, PetType.Dancer, GetSkillInfos(SkillTurnType.MyTurn)[1],
            SkillTurnType.MyTurn, true));
        yield return new WaitForSeconds(2f);
        StartCoroutine(Summer(assassinPrefab, PetType.Assassin, GetSkillInfos(SkillTurnType.EnemyTurn)[1],
            SkillTurnType.EnemyTurn, true));
    }


    public override void OnDie()
    {
        base.OnDie();
        dancer?.DestroyCharacter();
        assassin?.DestroyCharacter();
    }

    public override void DestroyCharacter()
    {
        base.DestroyCharacter();
        dancer?.DestroyCharacter();
        assassin?.DestroyCharacter();
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
    }

    public override void HandleAIPlay()
    {
        AlkawaDebug.Log(ELogCategory.AI, "HandleAIPlay");
        Info.GetMoveRange();
        var enemiesInRange = GpManager.GetEnemiesInRange(this, 2, DirectionType.All);
        if (dancer == null && Info.ActionPointsList.Count(p => p == 3) >= 2)
        {
            StartCoroutine(Summer(dancerPrefab, PetType.Dancer, GetSkillInfos(SkillTurnType.MyTurn)[1],
                SkillTurnType.MyTurn));
            Info.ReduceActionPoints();
        }
        else if (Info.ActionPointsList.Count(p => p == 3) >= 1 && enemiesInRange.Count > 0)
        {
            Enemy = enemiesInRange[0];
            HandleCastSkill(GetSkillInfos(SkillTurnType.MyTurn)[2], new List<Character> { Enemy });
            AlkawaDebug.Log(ELogCategory.AI, $"HandleAICastSkill: {GetSkillInfos(SkillTurnType.MyTurn)[2].name}");
        }
        else if (TryMoving())
        {
        }
        else
        {
            GpManager.HandleEndTurn();
        }
    }

    private IEnumerator Summer(GameObject type, PetType petType, SkillInfo skillInfo, SkillTurnType skillTurnType,
        bool dontNeedActionPoints = false)
    {
        HandleCastSkill(skillInfo, new List<Character> { this }, skillTurnType, dontNeedActionPoints);
        if (!dontNeedActionPoints) Info.ReduceActionPoints();
        yield return new WaitForSeconds(1f);
        SpawnEnemy(type, petType);
    }

    private void SpawnEnemy(GameObject gameObj, PetType type)
    {
        var cells = GpManager.MapManager.GetCellsWalkableInRange(Info.Cell, 6, DirectionType.All);
        if (cells is not { Count: > 0 }) return;
        int randomIndex = Random.Range(0, cells.Count);
        var selectedCell = cells[randomIndex];
        var go = Instantiate(gameObj);
        switch (type)
        {
            case PetType.Dancer:
                dancer = go.GetComponent<Dancer>();
                dancer.Initialize(selectedCell);
                dancer.owner = this;
                break;
            case PetType.Assassin:
                assassin = go.GetComponent<Assassin>();
                assassin.Initialize(selectedCell);
                assassin.owner = this;
                break;
        }
    }

    private enum PetType
    {
        Dancer,
        Assassin,
    }
}