using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding
{
    private readonly MapManager _map;

    private static readonly Dictionary<(int, int), DirectionType> EvenOffsets =
        new()
        {
            { (-1, -1), DirectionType.UpLeft },
            { (-1, 0), DirectionType.UpRight },
            { (0, -1), DirectionType.Left },
            { (0, 1), DirectionType.Right },
            { (1, -1), DirectionType.DownLeft },
            { (1, 0), DirectionType.DownRight },
        };

    private static readonly Dictionary<(int, int), DirectionType> OddOffsets = new()
    {
        { (-1, 0), DirectionType.UpLeft },
        { (-1, 1), DirectionType.UpRight },
        { (0, -1), DirectionType.Left },
        { (0, 1), DirectionType.Right },
        { (1, 0), DirectionType.DownLeft },
        { (1, 1), DirectionType.DownRight },
    };

    public Pathfinding(MapManager map)
    {
        _map = map;
    }

    private static List<Cell> ReconstructPath(Dictionary<Cell, Cell> cameFrom, Cell current)
    {
        var path = new List<Cell> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    private List<Cell> GetNeighbors(Cell hex, DirectionType allowedDirections)
    {
        int r = hex.CellPosition.x;
        int c = hex.CellPosition.y;
        bool isOdd = (r % 2) != 0;

        var offsets = isOdd ? OddOffsets : EvenOffsets;
        var neighbors = new List<Cell>();

        foreach (var offset in offsets)
        {
            int nr = r + offset.Key.Item1;
            int nc = c + offset.Key.Item2;
            if (nr >= 0 && nc >= 0 && nr < _map.MapSize.y && nc < _map.MapSize.x)
            {
                var neighbor = _map.GetCell(new Vector2Int(nr, nc));
                if (neighbor != null && IsDirectionAllowed(offset.Value, allowedDirections))
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    private static bool IsDirectionAllowed(DirectionType neighborDir, DirectionType allowed)
    {
        if (allowed.HasFlag(DirectionType.All))
            return true;
        if (allowed.HasFlag(neighborDir))
            return true;
        if ((neighborDir == DirectionType.UpLeft || neighborDir == DirectionType.UpRight) &&
            allowed.HasFlag(DirectionType.Up))
            return true;
        if ((neighborDir == DirectionType.DownLeft || neighborDir == DirectionType.DownRight) &&
            allowed.HasFlag(DirectionType.Down))
            return true;
        return false;
    }

    private static float GetHeuristic(Cell a, Cell b)
    {
        var aCube = OddRToCube(a.CellPosition.x, a.CellPosition.y);
        var bCube = OddRToCube(b.CellPosition.x, b.CellPosition.y);
        var dist = (Mathf.Abs(aCube.x - bCube.x) + Mathf.Abs(aCube.y - bCube.y) + Mathf.Abs(aCube.z - bCube.z)) / 2.0f;
        return dist;
    }

    private static (int x, int y, int z) OddRToCube(int r, int c)
    {
        var x = c - (r - (r & 1)) / 2;
        var z = r;
        var y = -x - z;
        return (x, y, z);
    }

    private class HexNode
    {
        public readonly Cell Hex;
        public float F;

        public HexNode(Cell hex, float f)
        {
            Hex = hex;
            F = f;
        }
    }

    public List<Cell> FindPath(Cell startHexagon, Cell endHexagon, DirectionType directionType)
    {
        if (startHexagon == null || endHexagon == null)
            return null;

        var openSet = new List<HexNode>();
        var cameFrom = new Dictionary<Cell, Cell>();
        var gScore = new Dictionary<Cell, float>();
        var fScore = new Dictionary<Cell, float>();

        gScore[startHexagon] = 0;
        fScore[startHexagon] = GetHeuristic(startHexagon, endHexagon);
        openSet.Add(new HexNode(startHexagon, fScore[startHexagon]));

        while (openSet.Count > 0)
        {
            openSet.Sort((a, b) => a.F.CompareTo(b.F));
            var currentNode = openSet[0];
            openSet.RemoveAt(0);

            if (currentNode.Hex == endHexagon)
            {
                return ReconstructPath(cameFrom, currentNode.Hex);
            }

            var neighbors = GetNeighbors(currentNode.Hex, directionType);
            foreach (var neighbor in neighbors.Where(n => n.CellType == CellType.Walkable))
            {
                var tentativeGScore = gScore.TryGetValue(currentNode.Hex, out var value) ? value + 1 : Mathf.Infinity;
                var currentGScore = gScore.GetValueOrDefault(neighbor, Mathf.Infinity);

                if (!(tentativeGScore < currentGScore))
                    continue;

                cameFrom[neighbor] = currentNode.Hex;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + GetHeuristic(neighbor, endHexagon);

                bool inOpenSet = false;
                foreach (var t in openSet.Where(t => t.Hex == neighbor))
                {
                    t.F = fScore[neighbor];
                    inOpenSet = true;
                    break;
                }

                if (!inOpenSet)
                {
                    openSet.Add(new HexNode(neighbor, fScore[neighbor]));
                }
            }
        }

        return null;
    }

    public HashSet<Cell> GetHexagonsInMoveRange(Cell center, int maxMoves,
        DirectionType directionType)
    {
        if (center == null || maxMoves < 0)
            return new HashSet<Cell>();

        var reachable = new HashSet<Cell>();
        var visited = new HashSet<Cell>();
        var queue = new Queue<(Cell cell, int moves)>();
        visited.Add(center);
        queue.Enqueue((center, 0));
        while (queue.Count > 0)
        {
            var (current, moves) = queue.Dequeue();
            reachable.Add(current);

            if (moves == maxMoves)
                continue;
            foreach (var neighbor in GetNeighbors(current, directionType))
            {
                if (visited.Contains(neighbor))
                    continue;

                if (neighbor.CellType != CellType.Walkable)
                    continue;
                visited.Add(neighbor);
                queue.Enqueue((neighbor, moves + 1));
            }
        }

        reachable.Remove(center);
        return reachable;
    }

    public HashSet<Cell> GetHexagonsInAttack(Cell center, int radius, DirectionType directionType)
    {
        if (center == null || radius < 0) return new HashSet<Cell>();

        var results = new HashSet<Cell>();
        var visited = new HashSet<Cell>();
        var queue = new Queue<(Cell hex, int dist)>();

        visited.Add(center);
        queue.Enqueue((center, 0));

        while (queue.Count > 0)
        {
            var (currentHex, dist) = queue.Dequeue();
            results.Add(currentHex);

            if (dist >= radius) continue;
            var neighbors = GetNeighbors(currentHex, directionType);
            foreach (var neighbor in neighbors.Where(n => !visited.Contains(n)))
            {
                visited.Add(neighbor);
                queue.Enqueue((neighbor, dist + 1));
            }
        }

        return results;
    }
}
