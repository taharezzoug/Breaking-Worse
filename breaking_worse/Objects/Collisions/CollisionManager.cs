using System;
using System.Collections.Generic;
using breaking_worse.Utility;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Collisions;

public class CollisionManager(bool[,] matrix = null)
{
    // const for degree, to avoid sonar warnings
    private const int Degree360 = 360;
    private readonly Vector2 _axisX = new(1, 0);
    
    // width and height of a cell in pixels
    private const int CellSize = 32;
    
    // dimensions of the Tiled Map
    private const int GridWidth = 250;
    private const int GridHeight = 140;
    
    // maps cell id to list of GameObjects
    public readonly Dictionary<int, List<Collider>> Grid = new();
    // maps colliders to cell id, so that update method can be implemented more efficiently
    private readonly Dictionary<Collider, int> _lastCellIds = new();

    /// <summary>
    /// checks for each given collider whether it has moved and then puts in newCell, otherwise leaves in old cell
    /// </summary>
    public void update(List<AGameObject> collidingObjects)
    {
        foreach (var collidingObject in collidingObjects)
        {
            var collider = collidingObject.getComponent<Collider>();
            var centerPoint = collider.DynamicHitBox.CenterPoint;
            var newCellId = getCellId(centerPoint.X, centerPoint.Y);
            
            if (_lastCellIds.TryGetValue(collider, out var oldCellId))
            {
                // checks if collider has moved, if not continue
                if (oldCellId == newCellId)
                    continue;
                
                Grid[oldCellId].Remove(collider);
            }
            insertCollider(collider, newCellId);
        }
    }

    public void removeCollider(Collider collider)
    {
        if (_lastCellIds.TryGetValue(collider, out var cellId))
            Grid[cellId].Remove(collider);
    }
    
    private int getCellId(float x, float y)
    {
        var cellX = (int)(x / CellSize);
        var cellY = (int)(y / CellSize);
        return cellX + cellY * GridWidth;
    }

    /// <summary>
    /// inserts the given collider in the grid
    /// </summary>
    private void insertCollider(Collider collider, int cellId)
    {
        if (!Grid.TryGetValue(cellId, out var colliders))
        {
            colliders = [];
            Grid[cellId] = colliders;
        }
        colliders.Add(collider);
        _lastCellIds[collider] = cellId;
    }
    
    public void insertCollider(Collider collider)
    {
        var centerPoint = collider.DynamicHitBox.CenterPoint;
        insertCollider(collider, getCellId(centerPoint.X, centerPoint.Y));
    }
    
    
    /// <summary>
    /// returns a list of colliders that are in the radius of the given collider
    /// runs quite efficiently
    /// </summary>
    public List<Collider> getNeighborsInRadius(Collider collider, int radius)
    {
        List<Collider> neighbors = [];
        
        var centerPoint = collider.DynamicHitBox.CenterPoint;
        var cellX = (int)(centerPoint.X / CellSize);
        var cellY = (int)(centerPoint.Y / CellSize);
        
        for (var dx = -radius; dx <= radius; dx++)
        {
            var neighborCellX = cellX + dx;
            // check if calculated neighbor cell is inside the map coords
            if (neighborCellX is < 0 or >= GridWidth) continue;
            
            for (var dy = -radius; dy <= radius; dy++)
            {
                var neighborCellY = cellY + dy;
                // check if calculated neighbor cell is inside the map coords
                if (neighborCellY is < 0 or >= GridHeight) continue;
    
                var neighborCellId = neighborCellX + neighborCellY * GridWidth;
                if (!Grid.TryGetValue(neighborCellId, out var colliders)) continue;
                neighbors.AddRange(colliders);
            }
        }
        neighbors.Remove(collider);
        return neighbors;
    }
    
    /// <summary>
    /// returns a list of colliders that are in the fov of the given collider
    /// </summary>
    public List<Collider> getNeighborsInFov(Collider collider, int radius, Vector2 facing, int fov, bool onlyCombatants = false)
    {
        List<Collider> neighbors = [];
        
        var centerPoint = collider.DynamicHitBox.CenterPoint;
        var cellX = (int)(centerPoint.X / CellSize);
        var cellY = (int)(centerPoint.Y / CellSize);
        
        // get angle between facing direction an x-axis
        var angle = _axisX.angle(facing);
        // normalize angle to be within [0,360)
        angle = OwnMath.mod(angle, Degree360);
        
        // iterate over all tiles within the radius and add them to the list if they are within the fov around the given angle
        for (var dx = -radius; dx <= radius; dx++)
        {
            var neighborCellX = cellX + dx;
            // check if calculated neighbor cell is inside the map coords
            if (neighborCellX is < 0 or >= GridWidth) continue;
            
            for (var dy = -radius; dy <= radius; dy++)
            {
                var neighborCellY = cellY + dy;
                // check if calculated neighbor cell is inside the map coords
                if (neighborCellY is < 0 or >= GridHeight) continue;
    
                var neighborCellId = neighborCellX + neighborCellY * GridWidth;
                if (!Grid.TryGetValue(neighborCellId, out var colliders)) continue;
                
                if (isInFov(angle, fov, new Vector2(neighborCellX - cellX, neighborCellY - cellY)))
                {
                    foreach (var currentCollider in colliders)
                    {
                        if (onlyCombatants && !currentCollider.IsCombatant) continue;

                        // Perform Line-of-Sight (LOS) check
                        // Triple to check to fix positions that are too close to wall according to Y axis
                        if (hasClearLineOfSight(cellX, cellY, neighborCellX, neighborCellY)
                            || hasClearLineOfSight(cellX, cellY + 1, neighborCellX, neighborCellY) 
                            || hasClearLineOfSight(cellX, cellY - 1, neighborCellX, neighborCellY))
                        {
                            neighbors.Add(currentCollider);
                        }
                    }
                }
            }
        }
        neighbors.Remove(collider);
        return neighbors;
    }

    // Takes origin point, and destination and evaluates using the collisionWithoutFence matrix
    private bool hasClearLineOfSight(int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (x0 != x1 || y0 != y1)
        {
            // Stop checking one step before reaching the target
            if (x0 == x1 && y0 == y1) 
                return true; 

            // If this is a wall, return false (excluding the last cell)
            if (matrix[x0, y0]) 
                return false;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }

        return true;
    }
    
    private bool isInFov(int angle, int fov, Vector2 neighborVector)
    { 
        // calculate the angle between the vector that connects the collider with the neighbor cell and the x-axis
        var neighborAngle = _axisX.angle(neighborVector);
        
        // calculate min and max angles of the fov
        var minAngle = OwnMath.mod(angle - fov / 2, Degree360);
        var maxAngle = OwnMath.mod(angle + fov / 2, Degree360);

        // check if neighborAngle is within the FOV
        if (minAngle < maxAngle)
            return neighborAngle >= minAngle && neighborAngle <= maxAngle;
        // handle the case where the FOV spans across 0°/360° boundary
        return neighborAngle >= minAngle || neighborAngle <= maxAngle;
    }
    
    /// <summary>
    /// returns list of coordinates of the tiles in radius of the collider (coordinates in number of tiles, not pixels)
    /// </summary>
    public List<(int, int)> getNeighborTilesInRadius(Collider collider, int radius)
    {
        List<(int, int)> neighborTiles = [];
        
        var centerPoint = collider.DynamicHitBox.CenterPoint;
        var cellX = (int)(centerPoint.X / CellSize);
        var cellY = (int)(centerPoint.Y / CellSize);
        
        for (var dx = -radius; dx <= radius; dx++)
        {
            var neighborCellX = cellX + dx;
            // check if calculated neighbor cell is inside the map coords
            if (neighborCellX is < 0 or >= GridWidth) continue;
            
            for (var dy = -radius; dy <= radius; dy++)
            {
                var neighborCellY = cellY + dy;
                // check if calculated neighbor cell is inside the map coords
                if (neighborCellY is < 0 or >= GridHeight) continue;
                neighborTiles.Add((neighborCellX, neighborCellY));
            }
        }
        return neighborTiles;
    }

    /// <summary>
    /// returns list of coordinates of the tiles in radius of the collider (coordinates in number of tiles, not pixels)
    /// </summary>
    public List<(int, int)> getNeighborTilesInFov(Collider collider, int radius, Vector2 facing, int fov)
    {
        List<(int, int)> neighborTiles = [];

        // get angle between facing direction an x-axis
        var angle = _axisX.angle(facing);
        // normalize angle to be within [0,360)
        angle = OwnMath.mod(angle, Degree360);
        
        var centerPoint = collider.DynamicHitBox.CenterPoint;
        var cellX = (int)(centerPoint.X / CellSize);
        var cellY = (int)(centerPoint.Y / CellSize);
        
        // iterate over all tiles within the radius and add them to the list if they are within the fov around the given angle
        for (var dx = -radius; dx <= radius; dx++)
        {
            var neighborCellX = cellX + dx;
            // check if calculated neighbor cell is inside the map coords
            if (neighborCellX is < 0 or >= GridWidth) continue;
            
            for (var dy = -radius; dy <= radius; dy++)
            {
                var neighborCellY = cellY + dy;
                // check if calculated neighbor cell is inside the map coords
                if (neighborCellY is < 0 or >= GridHeight) continue;
                
                if (isInFov(angle, fov, new Vector2(neighborCellX - cellX, neighborCellY - cellY))) 
                    neighborTiles.Add((neighborCellX, neighborCellY));
            }
        }
        return neighborTiles;
    }
}
