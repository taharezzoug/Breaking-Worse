using System;
using breaking_worse.Objects.Combat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Collisions;

// Component for Collision
public class Collider : IComponent
{
    private readonly GameManager _gameManager;
    private readonly CollisionManager _collisionManager;
    
    public InactiveCombatant InactiveCombatant { get; }
    public bool IsCombatant => InactiveCombatant != null;
    public bool IsPolice { get; set; }
    public bool IsPlayer { get; set; }
    public bool IsCartel { get; set; }
    public bool IsNpc { get; set; }
    public bool IsTrash { get; set; }
    
    // if true, collider is not taken into account in collision calculation
    public bool ExcludeFromCollisions { get; set; }
    
    // HitBox that is used for non-map Collisions, like interacting, fighting, colliding with other non-static gameObjects
    // (normally covers full image of gameObject) and map collisions
    public HitBox DynamicHitBox { get; }
    
    // stuff for drawing neighbors
    private const int CellSize = 32;
    private const float Opacity = 0.4f;
    private readonly Texture2D _collisionTexture;
    
    public Collider(GameManager gameManager, Func<Vector2> getPosition, 
         Vector2 dynamicHitBoxPositionOffSet, InactiveCombatant inactiveCombatant = null, bool excludeFromCollisions = false)
    {
        _gameManager = gameManager;
        _collisionManager = gameManager.ScreenManager.InGameScreen.CollisionManager;
        ExcludeFromCollisions = excludeFromCollisions;
        InactiveCombatant = inactiveCombatant;
        
        DynamicHitBox = new HitBox(() => getPosition() + dynamicHitBoxPositionOffSet);
        
        _collisionManager.insertCollider(this);
        
        _collisionTexture = new Texture2D(_gameManager.GraphicsDeviceManager.GraphicsDevice, 1, 1);
        _collisionTexture.SetData([Color.White]);
    }

    // should only be used to check for single colliders, never use this method in a loop!
    public bool isInRadius(Collider collider, int radius)
    {
        return _collisionManager.getNeighborsInRadius(this, radius).Contains(collider);
    }
    
    // should only be used to check for single colliders, never use this method in a loop!
    public bool isInFov(Collider collider, int radius, Vector2 facing, int fov)
    {
        return _collisionManager.getNeighborsInFov(this, radius, facing, fov).Contains(collider);
    }
    
    // returns the vector with maximal length (facing in direction) by which the corresponding GameObject can move so that it does not collide
    public Vector2 scaleDirectionByCollisions(Vector2 direction, GameTime gameTime, bool includeMapCollisions = true)
    {
        
        var futurePosition = DynamicHitBox.CenterPoint + direction;
        var maxDirection = direction;
        
        var centerPoint = this.DynamicHitBox.CenterPoint;
        var cellX = (int)(centerPoint.X / CellSize);
        var cellY = (int)(centerPoint.Y / CellSize);

        // in what radius (in tiles) to check for collisions
        int radius = 2;
        
        if (ExcludeFromCollisions) return direction;
        
        
        // check collisions again after sliding
        for (var dx = -radius; dx <= radius; dx++)
        {
            var neighborCellX = cellX + dx;
            if (neighborCellX is < 0 or >= GameManager.MapWidth) continue;

            for (var dy = -radius; dy <= radius; dy++)
            {
                var neighborCellY = cellY + dy;
                if (neighborCellY is < 0 or >= GameManager.MapHeight) continue;

                var neighborCellId = neighborCellX + neighborCellY * GameManager.MapWidth;
                
                // collisions with dynamic objects
                if (_gameManager.ScreenManager.InGameScreen.CollisionManager.Grid.TryGetValue(neighborCellId,
                        out var colliders))
                {
                    foreach (var collider in colliders)
                    {
                        var vectorBetweenColliders = collider.DynamicHitBox.CenterPoint - futurePosition;
                        var overlap = (Math.Max(DynamicHitBox.Width, DynamicHitBox.Height) + 
                                       Math.Max(collider.DynamicHitBox.Width, collider.DynamicHitBox.Height))/2f - vectorBetweenColliders.Length();
                        
                        if (overlap < 1 || collider.ExcludeFromCollisions || collider == this) continue;
                        vectorBetweenColliders.Normalize();
                        maxDirection -= vectorBetweenColliders * overlap * gameTime.ElapsedGameTime.Milliseconds / 30f;
                    }
                }
                // collisions with tiled objects
                if (_gameManager.ScreenManager.InGameScreen.Map.Grid.TryGetValue(neighborCellId, out var tiledObjects))
                {
                    foreach (var collisionFrame in tiledObjects)
                    {
                        var xDistanceBetweenColliders = collisionFrame.x + collisionFrame.width / 2f - futurePosition.X;
                        var yDistanceBetweenColliders = collisionFrame.y + collisionFrame.height / 2f - futurePosition.Y;
                        var xOverlap = (DynamicHitBox.Width + collisionFrame.width) / 2f - Math.Abs(xDistanceBetweenColliders);
                        var yOverlap = (DynamicHitBox.Height + collisionFrame.height) / 2f - Math.Abs(yDistanceBetweenColliders);
                        
                        if (xOverlap < 0.1f || yOverlap < 0.1f) continue;
                        
                        if (yOverlap > xOverlap) maxDirection.X -= Math.Sign(xDistanceBetweenColliders) * xOverlap * gameTime.ElapsedGameTime.Milliseconds / 100f;
                        else if (yOverlap < xOverlap) maxDirection.Y -= Math.Sign(yDistanceBetweenColliders) * yOverlap * gameTime.ElapsedGameTime.Milliseconds / 100f;
                    }
                }
            }
        }
        
        // clamp very small values to zero, to encounter floating point precision loss
        // doesn't corrupt movement, unless running at enormously high fps (not realistic)
        const float threshold = 1e-3f;
        if (Math.Abs(maxDirection.X) < threshold)
            maxDirection.X = 0;
        if (Math.Abs(maxDirection.Y) < threshold)
            maxDirection.Y = 0;
        
        return maxDirection;
    }


    public void drawHitBox()
    {
        var collisionTexture = new Texture2D(_gameManager.GraphicsDeviceManager.GraphicsDevice, 1, 1);
        collisionTexture.SetData([Color.White]);
        var dynamicHitBox = DynamicHitBox.convertToRectangle();

        const float dynamicHitBoxOpacity = 0.4f;
        //_gameManager.SpriteBatch.Draw(collisionTexture, dynamicHitBox, dynamicHitBoxOpacity * Color.Blue);
        _gameManager.SpriteBatch.Draw(_gameManager.AssetManager.Images["FOVIndicatorCircleWhite"], dynamicHitBox, dynamicHitBoxOpacity * Color.Blue);
    }
    
    public void drawTileCollisionHitBox()
    {
        var collisionTexture = new Texture2D(_gameManager.GraphicsDeviceManager.GraphicsDevice, 1, 1);
        collisionTexture.SetData([Color.White]);
        var dynamicHitBox = DynamicHitBox.convertToRectangle();

        const float dynamicHitBoxOpacity = 0.4f;
        _gameManager.SpriteBatch.Draw(collisionTexture, dynamicHitBox, dynamicHitBoxOpacity * Color.Blue);
    }
    
    public void drawNeighboursInRadius(int radius)
    {
        foreach (var tile in _collisionManager.getNeighborTilesInRadius(this, radius))
            _gameManager.SpriteBatch.Draw(_collisionTexture, new Rectangle(tile.Item1 * CellSize, tile.Item2 * CellSize, CellSize, CellSize), Opacity * Color.CornflowerBlue);
    }

    public void drawNeighboursInFov(int radius, Vector2 facing, int fov)
    {
        foreach (var tile in _collisionManager.getNeighborTilesInFov(this, radius, facing, fov))
            _gameManager.SpriteBatch.Draw(_collisionTexture, new Rectangle(tile.Item1 * CellSize, tile.Item2 * CellSize, CellSize, CellSize), Opacity * Color.CornflowerBlue);
    }
}
