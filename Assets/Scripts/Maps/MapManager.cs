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

    private HashSet<Cell> _moveRange = new();
    public HashSet<Cell> SkillRange { get; set; } = new();
    
    private void Awake()
    {
        foreach (var cell in cells)
        {
            cell.Value.gameObject.SetActive(false);
        }
    }

    public void Initialize()
    {
        _completed = 0;
        var count = 0;
        foreach (var cell in cells)
        {
            cell.Value.gameObject.SetActive(true);
            var delay = count * DelayIncrement;
            cell.Value.Initialize(delay, OnLoadMapFinished);
            count++;
        }
    
        _pathfinding = new Pathfinding(this);
    }

    private void OnLoadMapFinished()
    {
        _completed++;
        if (_completed >= cells.Count)
        {
            GameplayManager.Instance.LoadCharacter();
        }
    }

    public Cell GetCell(Vector2Int position)
    {
        return cells[position];
    }


    public void ShowMoveRange(Cell cell, int range)
    {
        _moveRange = _pathfinding.GetHexagonsInRange(cell, range);
        foreach (var item in _moveRange)
        {
            item.ShowMoveRange();
        }
    }

    public List<Cell> GetCellsWalkableInRange(Cell cell, int range)
    {
        var allCells = _pathfinding.GetHexagonsInRange(cell, range);
        return allCells.Where(c => c.CellType == CellType.Walkable).ToList();
    }

    public void HideMoveRange()
    {
        if (_moveRange == null || _moveRange.Count == 0) return;
        foreach (var item in _moveRange)
        {
            item.HideMoveRange();
        }

        _moveRange.Clear();
    }


    public void ShowSkillRange(Cell cell, int range)
    {
        SkillRange = _pathfinding.GetHexagonsInRange(cell, range);
        foreach (var item in SkillRange)
        {
            item.ShowSkillRange();
        }
    }

    public List<Character> GetCharacterInRange(Cell cell, int range)
    {
        var allCells = _pathfinding.GetHexagonsInRange(cell, range);
        return (from item in allCells where item.CellType == CellType.Character select item.Character).ToList();
    }

    public void HideSkillRange()
    {
        if (SkillRange == null || SkillRange.Count == 0) return;
        foreach (var item in SkillRange)
        {
            item.HideSkillRange();
        }

        SkillRange.Clear();
    }

    public List<Cell> FindPath(Cell startCell, Cell endCell)
    {
        return _pathfinding.FindPath(startCell, endCell);
    }

    public bool CanMove(Cell cell)
    {
        return _moveRange != null && _moveRange.Contains(cell);
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