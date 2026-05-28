using System;
using System.Collections.Generic;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Animations;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Combat;
using breaking_worse.Objects.Player;
using breaking_worse.Objects.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Enemy;

public abstract class AEnemy : AGameObject
{ 
    private const float ImageScalingFactor = 1f;

    // HitBox stuff
    private const int DynamicHitBoxWidth = 20;
    private const int DynamicHitBoxHeight = 20;
    
    // combat
    private const int Radius = 8;
    private const int Fov = 60;
    protected Weapon EquippedWeapon { get; set; } = Weapon.Fist;

    protected bool IsAlerted;
    
    protected bool IsPatrolling;
    protected bool IsChilling;
    protected List<(int, int)> PatrolPoints;
    protected int PatrolPointIndex;
    
    protected List<(int, int)> Path;
    private List<(int, int)> _lastPath;
    protected int PathIndex;
    
    protected const int PositionScalingFactor = 32;
    
    protected float PathfindingCooldown;
    private readonly Random _random = new();
    private const int PathfindingIntervalLowerBound = 1000; // in milliseconds
    private const int PathfindingIntervalUpperBound = 2000; // in milliseconds


    protected Vector2 DirectionAnimation;

    protected AEnemy(GameManager gameManager, Vector2 position) : base(gameManager)
    {
        var multiplier = GameManager.SettingsManager.getDifficultySettings().EnemyHealthMultiplier;
        Position = position;
        addComponent(new Animatable());
        addComponent(new ActiveCombatant(gameManager, Radius, Fov, () => EquippedWeapon,getComponent<Animatable>(), (int) (10 * multiplier)));
        addComponent(new Collider(gameManager, () => Position, 
            -new Vector2(DynamicHitBoxWidth / 2f, DynamicHitBoxHeight / 2f),
            getComponent<ActiveCombatant>()));
        
        var dynamicHitBox = getComponent<Collider>().DynamicHitBox;
        dynamicHitBox.Width = DynamicHitBoxWidth;
        dynamicHitBox.Height = DynamicHitBoxHeight;
    }

    public abstract void populatePatrolPath(List<(int, int)> path = null);
    
    public abstract void chase(GameTime gameTime);
    
    public abstract void move(GameTime gameTime, Vector2 direction);

    public override void update(GameTime gameTime)
    {
        if (getComponent<ActiveCombatant>().shouldDespawn())
            GameManager.ScreenManager.InGameScreen.GameObjectManager.removeGameObject(this);
        getComponent<Animatable>().CurrentAnimation.update(gameTime);
    }
    
    public override void render(GameTime gameTime)
    {
        // currently selected animation that should be drawn
        var currentAnimation = getComponent<Animatable>().CurrentAnimation;
        
        // rectangle that represents the image on the SpriteSheet that is currently needed for animation
        var sourceRectangle = currentAnimation.CurrentRectangle;
        
        // offset to position of the rendered image, so that the actual position is in the middle of the feet
        var renderPositionOffSet = new Point(-(int)(sourceRectangle.Width * ImageScalingFactor) / 2, -((int)(sourceRectangle.Height * ImageScalingFactor)));
        // scales the size of the rendered image by ImageScalingFactor
        var scaledSize = new Point((int)Math.Round(sourceRectangle.Width * ImageScalingFactor), (int)Math.Round(sourceRectangle.Height * ImageScalingFactor));
        // rectangle the image should be drawn in
        var destinationRectangle = new Rectangle(Position.ToPoint() + renderPositionOffSet, scaledSize);
        
        // check if last direction was to left or right and based on that mirror the image
        var spriteEffect = DirectionAnimation.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        float layerDepth = Position.Y / 100000f;
        
        GameManager.SpriteBatch.Draw(currentAnimation.SpriteSheet, destinationRectangle, sourceRectangle, Color.White, 0f, Vector2.Zero, spriteEffect, layerDepth);
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawHitBox();
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawTileCollisionRects, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawTileCollisionHitBox();
    }

    protected void findPath(Vector2 position, bool iAmStuck = false)
    {
        if (IsChilling)
            return;
        bool[,] cells = GameManager.ScreenManager.InGameScreen.Map.getCollisionMatrix();
        
        var stuckOffset = Point.Zero;
        if (iAmStuck)
        {
            if (position.X is < GameManager.MapWidth*32 and > 0 &&
                position.Y is < GameManager.MapHeight*32 and > 0)
            {
                stuckOffset = new(_random.Next(-1, 2), _random.Next(-1, 2));
                if (cells[(int)position.X / PositionScalingFactor + stuckOffset.X,
                        (int)position.Y / PositionScalingFactor + stuckOffset.Y])
                    stuckOffset = Point.Zero;
            } else stuckOffset = Point.Zero;
        }
        GameManager.ScreenManager.InGameScreen.Map.PathStart = Position;
        GameManager.ScreenManager.InGameScreen.Map.PathEnd = IsPatrolling 
            ? new Vector2(PatrolPoints[PatrolPointIndex].Item1, PatrolPoints[PatrolPointIndex].Item2)
            :position;

         
        if (!cells[(int)position.X / PositionScalingFactor,
                (int)position.Y / PositionScalingFactor])
        {
            PathFinder pathfinder = new PathFinder(GameManager.ScreenManager.InGameScreen.Map.MapWidth,
                GameManager.ScreenManager.InGameScreen.Map.MapHeight, cells);
            var destiny = IsPatrolling 
                ? (PatrolPoints[PatrolPointIndex].Item1, PatrolPoints[PatrolPointIndex].Item2)
                : ((int)position.X / PositionScalingFactor, (int)position.Y / PositionScalingFactor);
                    
            var newPath = pathfinder.findPath((stuckOffset.X + (int)Position.X / PositionScalingFactor,
                                                             stuckOffset.Y + (int)Position.Y / PositionScalingFactor), destiny);
            
            if (newPath != null)
            {
                Path = newPath;
                _lastPath = newPath;
                PathIndex = 0; // Reset only if a new path is found
            }
            else {
                // Keep following the last known path
                Path = _lastPath;
            }
        } 
        PathfindingCooldown = _random.Next(PathfindingIntervalLowerBound, PathfindingIntervalUpperBound) / 1000f;
    }

    protected void loadContent(string fileName)
    {
        loadAnimations(fileName);
    }

    private void loadAnimations(string filename)
    {
        const int rectangleWidth = 128; 
        const int rectangleHeight = 128;
        var animatable = getComponent<Animatable>();
        
        animatable.addAnimation("idle", new Animation(GameManager.AssetManager.Images[filename + "_Idle"], 6, rectangleWidth, rectangleHeight, 150, 0, true));
        animatable.addAnimation("running", new Animation(GameManager.AssetManager.Images[filename + "_Run"], 10, rectangleWidth, rectangleHeight, 60, 0, true));
        animatable.addAnimation("hurt", new Animation(GameManager.AssetManager.Images[filename + "_Hurt"], 4, rectangleWidth, rectangleHeight, 60, 2, false));
        animatable.addAnimation("die", new Animation(GameManager.AssetManager.Images[filename + "_Dead"], 5, rectangleWidth, rectangleHeight, 150, 3, false, true));
        
        animatable.addAnimation("Police_walk", new Animation(GameManager.AssetManager.Images["Police_Walk"], 7, rectangleWidth, rectangleHeight, 60, 0, true));
        animatable.addAnimation("Cartel_walk", new Animation(GameManager.AssetManager.Images["Cartel1_Walk"], 10, rectangleWidth, rectangleHeight, 60, 0, true));
        
        animatable.addAnimation("shoot", new Animation(GameManager.AssetManager.Images[filename + "_Shot"], 4, rectangleWidth, rectangleHeight, 50, 1, false));
        animatable.addAnimation("punch", new Animation(GameManager.AssetManager.Images[filename + "_Special"], filename switch {"Cartel1" => 3, "Police" => 13, _ => 0}, rectangleWidth, rectangleHeight, 60, 1, false));
        animatable.CurrentAnimationName = "idle";
        animatable.CurrentAnimation.start();
    }
    
    public List<(int, int)> getRoute() => PatrolPoints;
}
