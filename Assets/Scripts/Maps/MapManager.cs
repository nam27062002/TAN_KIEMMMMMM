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
    private const float DelayIncrement = 0.01f;
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
            var delay = count * DelayIncrement;
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
        return cells[position];
    }

    public HashSet<Cell> GetHexagonsInMoveRange(Cell cell, int range)
    {
        return _pathfinding.GetHexagonsInMoveRange(cell, range);
    }

    public HashSet<Cell> GetHexagonsInAttack(Cell cell, int range)
    {
        return _pathfinding.GetHexagonsInAttack(cell, range);
    }

    public List<Cell> GetCellsWalkableInRange(Cell cell, int range)
    {
        var allCells = _pathfinding.GetHexagonsInMoveRange(cell, range);
        return allCells.Where(c => c.CellType == CellType.Walkable).ToList();
    }

    public List<Cell> GetCellsWalkableInRange(Cell cell, int range, DirectionType directionType)
    {
        if (directionType == DirectionType.All)
        {
            return GetCellsWalkableInRange(cell, range);
        }

        if (directionType == DirectionType.None || cell == null)
        {
            return new List<Cell>();
        }

        var result = new List<Cell>();
        var directionVector = GetDirectionVector(directionType);
        var startPos = cell.CellPosition;

        for (var i = 1; i <= range; i++)
        {
            var newPos = startPos + directionVector * i;
            if (cells.TryGetValue(newPos, out Cell nextCell))
            {
                if (nextCell.CellType == CellType.Walkable)
                {
                    result.Add(nextCell);
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return result;
    }


    private static Vector2Int GetDirectionVector(DirectionType direction)
    {
        return direction switch
        {
            DirectionType.Up => new Vector2Int(1, 0),
            DirectionType.Down => new Vector2Int(-1, 0),
            DirectionType.Left => new Vector2Int(0, -1),
            DirectionType.Right => new Vector2Int(0, 1),
            DirectionType.UpRight => new Vector2Int(1, 1),
            DirectionType.UpLeft => new Vector2Int(1, -1),
            DirectionType.DownRight => new Vector2Int(-1, 1),
            DirectionType.DownLeft => new Vector2Int(-1, -1),
            _ => Vector2Int.zero
        };
    }


    public List<Character> GetCharacterInRange(Cell cell, int range)
    {
        var allCells = _pathfinding.GetHexagonsInAttack(cell, range);
        return (from item in allCells where item.CellType == CellType.Character select item.Character).ToList();
    }

    public HashSet<Character> GetCharactersInRange(Cell cell, SkillInfo info)
    {
        if (cell?.Character == null || info == null)
            return new HashSet<Character>();

        var self = cell.Character;
        return GetCharacterInRange(cell, info.range).Where(c => CheckDamageTarget(c, self, info.damageType))
            .ToHashSet();
    }

    public HashSet<Character> GetAllTypeInRange(Cell cell, CharacterType type, int range)
    {
        var allCells = _pathfinding.GetHexagonsInAttack(cell, range);
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

    public void DestroyMap()
    {
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