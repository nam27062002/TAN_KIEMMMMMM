using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding
{
    private readonly MapManager _map;

    public Pathfinding(MapManager map)
    {
        _map = map;
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

    private static float GetHeuristic(Cell a, Cell b)
    {
        var aCube = OddRToCube(a.CellPosition.x, a.CellPosition.y);
        var bCube = OddRToCube(b.CellPosition.x, b.CellPosition.y);
        var dist = (Mathf.Abs(aCube.x - bCube.x) + Mathf.Abs(aCube.y - bCube.y) + Mathf.Abs(bCube.z - aCube.z)) / 2.0f;
        return dist;
    }

    private static (int x, int y, int z) OddRToCube(int r, int c)
    {
        var x = c - (r - (r & 1)) / 2;
        var z = r;
        var y = -x - z;
        return (x, y, z);
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


    public List<Cell> FindPath(Cell startHexagon, Cell endHexagon)
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

            var neighbors = GetNeighbors(currentNode.Hex);
            foreach (var neighbor in neighbors.Where(neighbor => neighbor.CellType == CellType.Walkable))
            {
                var tentativeGScore = gScore.TryGetValue(currentNode.Hex, out var value) ? value + 1 : Mathf.Infinity;
                var currentGScore = gScore.GetValueOrDefault(neighbor, Mathf.Infinity);

                if (!(tentativeGScore < currentGScore)) continue;
                cameFrom[neighbor] = currentNode.Hex;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + GetHeuristic(neighbor, endHexagon);

                var inOpenSet = false;
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

    private List<Cell> GetNeighbors(Cell hex)
    {
        var r = hex.CellPosition.x;
        var c = hex.CellPosition.y;
        var isOdd = (r % 2) != 0;

        var offsets = isOdd
            ? new[]
            {
                new[] { -1, 0 },
                new[] { +1, 0 },
                new[] { 0, -1 },
                new[] { 0, +1 },
                new[] { -1, +1 },
                new[] { +1, +1 },
            }
            : new[]
            {
                new[] { -1, -1 },
                new[] { +1, -1 },
                new[] { 0, -1 },
                new[] { 0, +1 },
                new[] { -1, 0 },
                new[] { +1, 0 },
            };

        return (from off in offsets
            let nr = r + off[0]
            let nc = c + off[1]
            where nr >= 0 && nc >= 0 && nr < _map.MapSize.y && nc < _map.MapSize.x
            select _map.GetCell(new Vector2Int(nr, nc))
            into neighbor
            where neighbor != null
            select neighbor).ToList();
    }

    public HashSet<Cell> GetHexagonsInMoveRange(Cell center, int maxMoves, DirectionType direction)
    {
        if (center == null || maxMoves < 0)
            return new HashSet<Cell>();

        var reachable = new HashSet<Cell>();
        if (direction == DirectionType.All)
        {
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
                foreach (var neighbor in GetNeighbors(current).Where(n => n.CellType == CellType.Walkable))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, moves + 1));
                    }
                }
            }
        }
        else
        {
            foreach (var dir in GetDirections(direction))
            {
                Cell current = center;
                int steps = 0;
                while (steps < maxMoves)
                {
                    var neighbor = GetNeighborInDirection(current, dir);
                    if (neighbor == null || neighbor.CellType != CellType.Walkable)
                        break;
                    reachable.Add(neighbor);
                    current = neighbor;
                    steps++;
                }
            }
        }

        reachable.Remove(center);
        return reachable;
    }

    public HashSet<Cell> GetHexagonsInAttack(Cell center, int radius, DirectionType direction)
    {
        if (center == null || radius < 0) return new HashSet<Cell>();

        var results = new HashSet<Cell>();
        if (direction == DirectionType.All)
        {
            var visited = new HashSet<Cell>();
            var queue = new Queue<(Cell hex, int dist)>();
            visited.Add(center);
            queue.Enqueue((center, 0));
            while (queue.Count > 0)
            {
                var (currentHex, dist) = queue.Dequeue();
                results.Add(currentHex);

                if (dist >= radius) continue;
                foreach (var neighbor in GetNeighbors(currentHex).Where(neighbor => !visited.Contains(neighbor)))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, dist + 1));
                }
            }
        }
        else
        {
            foreach (var dir in GetDirections(direction))
            {
                Cell current = center;
                for (int step = 0; step < radius; step++)
                {
                    current = GetNeighborInDirection(current, dir);
                    if (current == null)
                        break;
                    results.Add(current);
                }
            }
        }

        results.Remove(center);
        return results;
    }

    private IEnumerable<DirectionType> GetDirections(DirectionType direction)
    {
        foreach (DirectionType dir in System.Enum.GetValues(typeof(DirectionType)))
        {
            if (dir != DirectionType.None && dir != DirectionType.All && direction.HasFlag(dir))
            {
                yield return dir;
            }
        }
    }

    private Cell GetNeighborInDirection(Cell current, DirectionType direction)
    {
        Vector2Int currentPos = current.CellPosition;
        bool isOdd = (currentPos.x % 2) != 0;
        Vector2Int offset = GetDirectionOffset(direction, isOdd);
        Vector2Int neighborPos = new Vector2Int(currentPos.x + offset.x, currentPos.y + offset.y);
        return _map.GetCell(neighborPos);
    }

    private Vector2Int GetDirectionOffset(DirectionType direction, bool isOddRow)
    {
        switch (direction)
        {
            case DirectionType.Left:
                return new Vector2Int(0, -1);
            case DirectionType.Right:
                return new Vector2Int(0, 1);
            case DirectionType.UpRight:
                return new Vector2Int(-1, 1);
            // case DirectionType.UpLeft:
            //     return new Vector2Int(-1, -1);
            // case DirectionType.DownRight:
            //     return new Vector2Int(1, 1);
            // case DirectionType.DownLeft:
            //     return isOddRow ? new Vector2Int(1, -1) : new Vector2Int(1, 0);
            default:
                return Vector2Int.zero;
        }
    }
}