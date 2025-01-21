using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding
{
    private readonly MapManager _map;

    public Pathfinding(MapManager map)
    {
        _map = map;
        Debug.Log("Gameplay Pathfinding: Initializing...");
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

        return (from off in offsets let nr = r + off[0] let nc = c + off[1] where nr >= 0 && nc >= 0 && nr < _map.MapSize.y && nc < _map.MapSize.x select _map.GetCell(new Vector2Int(nr, nc)) into neighbor where neighbor != null select neighbor).ToList();
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

    public HashSet<Cell> GetHexagonsInRange(Cell center, int radius)
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
            var neighbors = GetNeighbors(currentHex);
            foreach (var neighbor in neighbors.Where(neighbor => !visited.Contains(neighbor)))
            {
                visited.Add(neighbor);
                queue.Enqueue((neighbor, dist + 1));
            }
        }

        return results;
    }
}