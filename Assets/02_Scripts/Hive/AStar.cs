using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    private class Node
    {
        public Vector3Int pos;
        public int gCost;
        public int hCost;
        public Node parent;
        public int fCost => gCost + hCost;

        public Node(Vector3Int _pos)
        {
            pos = _pos;
        }
    }

    private static readonly Vector3Int[] Dirs4 = {
        new Vector3Int(0, 1, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0)
    };

    public static List<Vector2> FindPathWorld(Vector2 startWorld, Vector2 targetWorld)
    {
        if (HiveGrid.Instance == null) return new List<Vector2> { targetWorld };

        Vector3Int startCell = HiveGrid.Instance.WorldToCell(startWorld);
        Vector3Int targetCell = HiveGrid.Instance.WorldToCell(targetWorld);

        var cellPath = FindPath(startCell, targetCell);

        if (cellPath == null || cellPath.Count == 0)
        {
            // If no path is found, do NOT fallback to a direct line! Just stay put.
            return new List<Vector2>();
        }

        List<Vector2> worldPath = new List<Vector2>();
        for (int i = 0; i < cellPath.Count; i++)
        {
            worldPath.Add(HiveGrid.Instance.CellToWorld(cellPath[i]));
        }

        // Force the final waypoint to be the exact center of the cell, so they don't wander off-grid!
        return worldPath;
    }

    private static List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int targetPos)
    {
        if (!IsWalkable(targetPos))
        {
            // If target is literally inside a wall, try to find nearest walkable neighbor
            targetPos = GetNearestWalkable(targetPos);
            if (!IsWalkable(targetPos)) return null;
        }

        Node startNode = new Node(startPos);
        Node targetNode = new Node(targetPos);

        List<Node> openList = new List<Node> { startNode };
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

        int maxIterations = 1000;
        int iterations = 0;

        while (openList.Count > 0)
        {
            iterations++;
            if (iterations > maxIterations) break; // Safety net

            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost || openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost)
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode.pos);

            if (currentNode.pos == targetPos)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Vector3Int dir in Dirs4)
            {
                Vector3Int neighborPos = currentNode.pos + dir;

                if (closedSet.Contains(neighborPos)) continue;

                if (!IsWalkable(neighborPos))
                {
                    closedSet.Add(neighborPos);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + GetDistance(currentNode.pos, neighborPos);

                Node neighborNode = openList.Find(n => n.pos == neighborPos);
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos);
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = GetDistance(neighborPos, targetPos);
                    neighborNode.parent = currentNode;
                    openList.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = currentNode;
                }
            }
        }

        return null;
    }

    private static List<Vector3Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.pos);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private static int GetDistance(Vector3Int nodeA, Vector3Int nodeB)
    {
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public static bool IsWalkable(Vector3Int cell)
    {
        HiveTileType type = HiveGrid.Instance.GetType(cell);
        
        // Solid dirt and Bedrock are not walkable! Empty space and rooms ARE walkable.
        return type != HiveTileType.Hive && type != HiveTileType.Solid;
    }

    private static Vector3Int GetNearestWalkable(Vector3Int start)
    {
        // Simple 1-radius check
        foreach (var dir in Dirs4)
        {
            if (IsWalkable(start + dir)) return start + dir;
        }
        return start;
    }
}
