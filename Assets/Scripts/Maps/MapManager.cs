using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Transform cellHolder;
    [SerializeField] private SerializableDictionary<Vector2Int, Cell> cells = new();

    private int _completed;         
    private Pathfinding _pathfinding;
    public Vector2Int MapSize => mapSize;
    public SerializableDictionary<Vector2Int, Cell> Cells => cells;


    public event EventHandler OnLoadMapFinished;

    private void Awake()
    {
        foreach (var cell in cells)
        {
            cell.Value.gameObject.SetActive(false);
        }
    }

    public void Initialize()
    {
        _pathfinding = new Pathfinding(this);
        _completed = 0;
        var count = 0;
        foreach (var cell in cells)
        {
            cell.Value.gameObject.SetActive(true);
#if UNITY_EDITOR
            var delay = 0;
#else
            var delay = count * GameplayManager.Instance.LevelConfig.delayIncrement;
#endif
            cell.Value.Initialize(delay, HandleLoadMapFinished);
            count++;
        }

        AlkawaDebug.Log(ELogCategory.GAMEPLAY, "Map Manager initialized");
    }

    private void HandleLoadMapFinished()
    {
        _completed++;
        if (_completed < cells.Count) return;
        OnLoadMapFinished?.Invoke(this, EventArgs.Empty);
    }

    public Cell GetCell(Vector2Int position)
    {
        return cells.TryGetValue(position, out var cell) ? cell : null;
    }

    public HashSet<Cell> GetHexagonsInMoveRange(Cell cell, int range, DirectionType direction)
    {
        return _pathfinding.GetHexagonsInMoveRange(cell, range, direction);
    }

    public HashSet<Cell> GetAllHexagonInRange(Cell cell, int range)
    {
        return _pathfinding.GetHexagonsInAttack(cell,range, DirectionType.All);
    }

    public HashSet<Cell> GetHexagonsInAttack(Cell cell, SkillInfo skillInfo)
    {
        var allCells = _pathfinding.GetHexagonsInAttack(cell, skillInfo.range, skillInfo.directionType);
        var results = new HashSet<Cell>();
    
        foreach (var item in allCells)
        {
            if (item.CellType == CellType.Character)
            {
                bool isSameTeam = item.Character.Type == cell.Character.Type;
            
                if ((skillInfo.damageType == DamageTargetType.Team && isSameTeam) ||
                    (skillInfo.damageType == DamageTargetType.Enemies && !isSameTeam))
                {
                    results.Add(item);
                }
            }
            else if (item.CellType == CellType.Walkable)
            {
                if (skillInfo.damageType == DamageTargetType.Move)
                {
                    results.Add(item);
                }
            }
        }
    
        return results;
    }
    
    public List<Cell> GetCellsWalkableInRange(Cell cell, int range, DirectionType direction)
    {
        var allCells = _pathfinding.GetHexagonsInMoveRange(cell, range, direction);
        return allCells.Where(c => c.CellType == CellType.Walkable).ToList();
    }
    
    public List<Character> GetCharacterInRange(Cell cell, int range, DirectionType direction)
    {
        var allCells = _pathfinding.GetHexagonsInAttack(cell, range, direction);
        return (from item in allCells where item.CellType == CellType.Character select item.Character).ToList();
    }

    public HashSet<Character> GetCharactersInRange(Cell cell, SkillInfo info)
    {
        if (cell?.Character == null || info == null)
            return new HashSet<Character>();

        var self = cell.Character;
        return GetCharacterInRange(cell, info.range, info.directionType).Where(c => CheckDamageTarget(c, self, info.damageType))
            .ToHashSet();
    }

    public HashSet<Character> GetAllTypeInRange(Cell cell, CharacterType type, int range)
    {
        var allCells = _pathfinding.GetHexagonsInAttack(cell, range, DirectionType.All);
        var res = new HashSet<Character>();
        foreach (var item in allCells.Where(item =>
                     item.CellType == CellType.Character && item.Character.characterType == type))
        {
            res.Add(item.Character);
        }

        res.Remove(cell.Character);
        return res;
    }

    private static bool CheckDamageTarget(Character target, Character self, DamageTargetType damageType)
    {
        return (damageType.HasFlag(DamageTargetType.Self) && target == self) ||
               (damageType.HasFlag(DamageTargetType.Team) && target.Type == self.Type) ||
               (damageType.HasFlag(DamageTargetType.Enemies) && target.Type != self.Type);
    }

    public List<Cell> FindPath(Cell startCell, Cell endCell)
    {
        return _pathfinding.FindPath(startCell, endCell);
    }

    public List<Cell> FindShortestPath(Cell startCell, Cell endCell)
    {
        return _pathfinding.FindShortestPath(startCell, endCell);
    }
    
    public void DestroyMap()
    {
        foreach (var cell in cells.Values.ToList())
        {
            cell.DestroyCell();
        }
        cells.Clear();
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    [Button("Create Map")]
    private void CreateMap()
    {
        ClearMap();
        for (var row = 0; row < mapSize.y; row++)
        {
            for (var col = 0; col < mapSize.x; col++)
            {
                var xPos = col * offset.x;
                var yPos = row * offset.y;
                if (row % 2 != 0)
                {
                    xPos += offset.x / 2f;
                }

                var go = (GameObject)PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, transform);
                go.transform.parent = cellHolder;
                go.transform.position = new Vector3(xPos, yPos, 0);
                go.name = $"{row}_{col}";
                var cell = go.GetComponent<Cell>();
                cell.CellPosition = new Vector2Int(row, col);
                cells[cell.CellPosition] = cell;
            }
        }
    }

    [Button("Clear Map")]
    private void ClearMap()
    {
        foreach (var cell in cells)
        {
            DestroyImmediate(cell.Value.gameObject);
        }

        cells.Clear();
    }
#endif
}