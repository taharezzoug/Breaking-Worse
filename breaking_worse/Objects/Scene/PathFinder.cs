using System;
using System.Collections.Generic;
using System.Linq;

namespace breaking_worse.Objects.Scene;

public class PathFinder(int mapWidth, int mapHeight, bool[,] collisionMatrix)
{
    private int heuristic((int x, int y) a, (int x, int y) b)
    {
        // Manhattan distance as heuristic
        int dx = Math.Abs(a.x - b.x);
        int dy = Math.Abs(a.y - b.y);
        return 10 * Math.Max(dx, dy) + 4 * Math.Min(dx, dy);
    }

    public List<(int x, int y)> findPath((int x, int y) start, (int x, int y) destination)
    {
        var openSet = new PriorityQueue<(int x, int y), int>(); // Priority queue with cells and their f-score
        var openSetHash = new HashSet<(int x, int y)>(); // Auxiliary set to track elements in openSet
        openSet.Enqueue(start, 0);
        openSetHash.Add(start);

        var cameFrom = new Dictionary<(int x, int y), (int x, int y)>(); // To reconstruct the path

        var gScore = new Dictionary<(int x, int y), int>(); // Cost from start to a cell
        gScore[start] = 0;

        var fScore = new Dictionary<(int x, int y), int>(); // Estimated total cost from start to destination
        fScore[start] = heuristic(start, destination);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue(); // Get the cell with the lowest f-score
            openSetHash.Remove(current);    // Remove from the auxiliary hash set

            if (current == destination)
                return reconstructPath(cameFrom, current); // Reached the destination

            foreach (var neighbor in getNeighbors(current))
            {
                bool isDiagonal = Math.Abs(neighbor.x - current.x) == 1 && Math.Abs(neighbor.y - current.y) == 1;
                int movementCost = isDiagonal ? 14 : 10;
                int tentativeGScore = gScore[current] + movementCost;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + heuristic(neighbor, destination);

                    if (!openSetHash.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                        openSetHash.Add(neighbor); // Add to the auxiliary hash set
                    }
                }
            }
        }
        
        // If we exhaust the open set without finding the destination
        return null;
    }

    private List<(int x, int y)> getNeighbors((int x, int y) cell)
    {
        var neighbors = new List<(int x, int y)>
        {
            (cell.x - 1, cell.y), // Left
            (cell.x + 1, cell.y), // Right
            (cell.x, cell.y - 1), // Up
            (cell.x, cell.y + 1),  // Down
            (cell.x - 1, cell.y - 1), // Top-left
            (cell.x + 1, cell.y - 1), // Top-right
            (cell.x - 1, cell.y + 1), // Bottom-left
            (cell.x + 1, cell.y + 1)  // Bottom-right
        };

        // Filter neighbors that are out of bounds or blocked
        return neighbors.Where(isCellValid).ToList();
    }

    private bool isCellValid((int x, int y) cell)
    {
        return cell.x >= 0 && cell.x < mapWidth &&
               cell.y >= 0 && cell.y < mapHeight &&
               !collisionMatrix[cell.x, cell.y]; // Must not be blocked
    }

    private List<(int x, int y)> reconstructPath(Dictionary<(int x, int y), (int x, int y)> cameFrom, (int x, int y) current)
    {
        var path = new List<(int x, int y)> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
}
