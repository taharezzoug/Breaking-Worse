using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Animations;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Combat;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.Objects.Scene;
using breaking_worse.Screens.ScreenTypes.InGameScreens;
using breaking_worse.Sound;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using breaking_worse.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Interaction;

public class NpcBuyer : AGameObject
{
    private const float ImageScalingFactor = 1f;

    // HitBox dimensions
    private const int DynamicHitBoxWidth = 20;
    private const int DynamicHitBoxHeight = 20;

    // interaction radius
    private const int Radius = 2;

    private float _pathfindingCooldown;
    private readonly Random _random = new();
    private const int PathfindingIntervalLowerBound = 3000; // in MILLIseconds
    private const int PathfindingIntervalUpperBound = 4000; // in MILLIseconds
    
    public Dictionary<ItemName, int> Needs = new();
    private double _buyCooldown;
    private readonly int _restockTimer;
    private float _priceFactor;
    private int _originalScreenWidth;
    private float _scale;
    private bool _iAmStuck;
    
    private readonly float _movementSpeed = 100f;
    private readonly float _walkingSpeed = 50f;
    private readonly float _stop = 0f;
    private Vector2 _directionAnimation;

    private List<(int, int)> _positions;
    private int _positionIndex;
    private List<(int, int)> _path;
    private List<(int, int)> _lastPath;
    private int _pathIndex;
    private (int x, int y) _previousCell;
    
    private (int X, int Y) _flightPosition;
    private int _health;
    private bool _recentHit;

    private const int PositionScalingFactor = 32;

    private bool _isRunning;
    
    private readonly Sfx[] _sfxes;
    private bool _flag;
    private Pickables _pickable;
    private readonly List<string> _animations = ["Addict1", "Addict2","Addict3","Addict4"];

    public NpcBuyer(GameManager gameManager, Vector2 position, Dictionary<ItemName, int> items = null, List<(int X, int Y)> points = null, float areaFactor = 1) : base(gameManager)
    {
        Type = NpcType.NpcBuyer;
        _restockTimer = (int)(30 * GameManager.SettingsManager.getDifficultySettings().EnemyHealthMultiplier);
        Position = position;
        _scale = GameManager.SettingsManager.Resolution.Width / 1920f;
        _originalScreenWidth = GameManager.SettingsManager.Resolution.Width;

        _health = 5;

        _previousCell = ((int)Position.X / PositionScalingFactor, (int)Position.Y / PositionScalingFactor);
        
        setPoints(points);

        addComponent(new Dialogable(gameManager, createDialogueScreen, Radius));
        addComponent(new Animatable());
        addComponent(new InactiveCombatant(gameManager, _health, getComponent<Animatable>()));
        addComponent(new Collider(gameManager, () => Position,
            -new Vector2(DynamicHitBoxWidth / 2f, DynamicHitBoxHeight / 2f),
            getComponent<InactiveCombatant>()));
        
        getComponent<Collider>().IsNpc = true;
        var dynamicHitBox = getComponent<Collider>().DynamicHitBox;
        dynamicHitBox.Width = DynamicHitBoxWidth;
        dynamicHitBox.Height = DynamicHitBoxHeight;
        
        _priceFactor = areaFactor;
        
        setItems(items);
        
        _sfxes = [Sfx.Cough, Sfx.CocaineSniff, Sfx.Smoking];
    }

    private DialogueScreen createDialogueScreen(PlayerCharacterId playerCharacterId)
    {
        var r = new Random();
        GameManager.SoundManager.playSoundEffect(_sfxes[r.Next(_sfxes.Length)]);
        var dialogueScreen = playerCharacterId == PlayerCharacterId.Walt
            ? new DialogueScreen(GameManager, new Vector2(300 * _scale, (GameManager.SettingsManager.Resolution.Height + 550 * _scale) * 0.2f / _scale), [playerCharacterId], null, this, _priceFactor)
            : new DialogueScreen(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width - 300 * _scale, (GameManager.SettingsManager.Resolution.Height + 550 * _scale) * 0.2f / _scale), [playerCharacterId], null, this, _priceFactor);
            
        dialogueScreen.ButtonCollection.addTextElement("Buy", "Yo, ill take the usual", 600, 0.6f);
       
        foreach (var item in Needs)
            dialogueScreen.ButtonCollection.addButton(item.Key + "_sell", $"", 0.6f); //Text is updated in DialogueScreen
        
        dialogueScreen.ButtonCollection.isDialog(true);

        return dialogueScreen;
    }

    private void setItems(Dictionary<ItemName, int> items = null)
    {
        if (items == null)
        {
            Needs = [];
            Needs.Add(ItemName.MethSmall, 1);
            Needs.Add(ItemName.Meth, 1);
            Needs.Add(ItemName.Cocaine, 1);
            Needs.Add(ItemName.Crack, 1);
        }
        else
        {
            Dictionary<ItemName, int> newItems = [];
            foreach (var item in items)
            {
                if (item.Key == ItemName.MethSmall || item.Key == ItemName.Meth ||
                    item.Key == ItemName.Cocaine || item.Key == ItemName.Crack)
                {
                    newItems[item.Key] = item.Value;
                }
                else
                {
                    throw new Exception($"{item.Key} can not be set as Buyer item!");
                }
            }
            Needs = newItems;
        }
    }
    public void startCooldown()
    {
        _buyCooldown = _restockTimer;
    }
    private void restock()
    {
        foreach (var item in Needs.Keys.ToList())
        {
            Needs[item] = item switch
            {
                ItemName.MethSmall => 1,
                ItemName.Meth => 1,
                ItemName.Crack => 1,
                ItemName.Cocaine => 1,
                _ => Needs[item]
            };
        }
    }
    private void setPoints(List<(int, int)> path = null)
    {
        if (path != null)
            _positions = path;
        else
            _positions = [((int)(Position.X / PositionScalingFactor), (int)(Position.Y / PositionScalingFactor))];
    }

    private void goToNextSpot(GameTime gameTime)
    {
        if (_previousCell == _flightPosition)
        {
            _isRunning = false;
        }
        // Get the next target position from the path
        if (_path[_pathIndex] == _previousCell)
        {
            _pathIndex++;
        }
        
        if (_positions.Count > 1 && _positions[_positionIndex] == _previousCell)
        {
            _isRunning = false;
            _positionIndex = (_positionIndex + 1) % _positions.Count;
        }

        if (_positions.Count == 1 && _positions[0] == _previousCell)
        {
            _positionIndex = 0;
            _isRunning = false;
        }

        // Check if there's a path and if we haven't reached the end of it
        if (_path != null && _pathIndex < _path.Count)
        {
            (int x, int y) nextCell = _path[_pathIndex];
            Vector2 nextPosition = new Vector2(nextCell.x * PositionScalingFactor + PositionScalingFactor/2f, nextCell.y * PositionScalingFactor + PositionScalingFactor/2f);
            
            move(gameTime, nextPosition);

            // Check if the position has reached the destination
            if (Vector2.Distance(Position, nextPosition) < 10f)
            {
                _pathIndex++;
            }
        }
    }

    private void move(GameTime gameTime, Vector2 destination)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 direction = destination - Position;
        
        if (direction.LengthSquared() > 0.0001f)
            direction.Normalize();
        
        var speed = _isRunning ? _movementSpeed : _walkingSpeed;
        if (!getComponent<Dialogable>().IsClosed)
            speed = _stop;
        
        direction = direction * speed * deltaTime;
        
        var directionBeforeCollider = direction;
        direction = getComponent<Collider>().scaleDirectionByCollisions(direction, gameTime, false);
        _iAmStuck = Math.Abs(directionBeforeCollider.angle(direction)) > Math.PI / 2f;
        // control facing of character sprite
        if (direction.X != 0)
            _directionAnimation = direction;
        Position += direction;
        
        if (Vector2.Distance(Position, destination) < speed * deltaTime)
            Position = destination;
        _previousCell = ((int)Position.X / PositionScalingFactor, (int)Position.Y / PositionScalingFactor);
    }

    private void isPoliceInRange()
    {
        var cellSize = 32;
        var centerPoint = getComponent<Collider>().DynamicHitBox.CenterPoint;
        var cellX = (int)(centerPoint.X / cellSize);
        var cellY = (int)(centerPoint.Y / cellSize);
        int radius = 1;
        for (var dx = -radius; dx <= radius; dx++)
        {
            var neighborCellX = cellX + dx;
            if (neighborCellX is < 0 or >= GameManager.MapWidth) continue;
            for (var dy = -radius; dy <= radius; dy++)
            {
                var neighborCellY = cellY + dy;
                if (neighborCellY is < 0 or >= GameManager.MapHeight) continue;
                var neighborCellId = neighborCellX + neighborCellY * GameManager.MapWidth;

                if (GameManager.ScreenManager.InGameScreen.CollisionManager.Grid.TryGetValue(neighborCellId, out var colliders))
                {
                    foreach (var neighbor in colliders)
                    {
                        if (neighbor.IsCombatant && neighbor.IsPolice)
                        {
                            _isRunning = true;
                        }
                    }
                }
            }
        }
    }

    private void generateDrops()
    {
        bool isInRichArea = (Position.X > 5800 && Position.Y > 2630);
        int money = isInRichArea ? _random.Next(100, 300) : _random.Next(0, 80);
        
        List<PossiblePickables> poorDrugs = [PossiblePickables.Crack, PossiblePickables.MethSmall, PossiblePickables.Matches];
        List<PossiblePickables> richDrugs = [PossiblePickables.Cocaine, PossiblePickables.Meth];
        PossiblePickables drug = isInRichArea ? richDrugs[_random.Next(richDrugs.Count)] : poorDrugs[_random.Next(poorDrugs.Count)];
        
        List<(PossiblePickables, int)> drops = [(PossiblePickables.Money, money)];
        if (_random.Next(5) != 0) drops.Add((PossiblePickables.Matches, 1));
        if (_random.Next(4) == 0) drops.Add((drug, 1));
        
        _pickable = new Pickables(GameManager, Position, drops);
        GameManager.ScreenManager.InGameScreen.GameObjectManager.addPickable(_pickable);
    }

    public override void update(GameTime gameTime)
    {
        if (getComponent<InactiveCombatant>().Health > 0)
        {
            if (CanBeTalkedToWalt && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsHidden)
            {
                getComponent<Dialogable>().update(getComponent<Collider>(), PlayerCharacterId.Walt);
            }

            if (CanBeTalkedToJesse && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsHidden)
            {
                getComponent<Dialogable>().update(getComponent<Collider>(), PlayerCharacterId.Jesse);
            }
        }

        
        if (_buyCooldown <= 0)
            restock();
        else
            _buyCooldown -= gameTime.ElapsedGameTime.TotalSeconds;
        
        if (_originalScreenWidth != GameManager.SettingsManager.Resolution.Width)
        {
            _scale = GameManager.SettingsManager.Resolution.Width / 1920f;
            _originalScreenWidth = GameManager.SettingsManager.Resolution.Width;
        }
        if (getComponent<InactiveCombatant>().shouldDespawn())
            GameManager.ScreenManager.InGameScreen.GameObjectManager.removeGameObject(this);
        getComponent<Animatable>().CurrentAnimation.update(gameTime);

        if (getComponent<InactiveCombatant>().Health <= 0)
        {
            if (!_flag)
            {
                generateDrops();
                _flag = true;
            }
            return;
        }
        
        _pathfindingCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (!_recentHit && getComponent<InactiveCombatant>().Health < _health)
        {
            _isRunning = true;
            _recentHit = true;
            _health = getComponent<InactiveCombatant>().Health;
            _pathfindingCooldown = 0;
        }
        
        if (_pathfindingCooldown <= 0)
        {
                bool[,] cells = GameManager.ScreenManager.InGameScreen.Map.getCollisionMatrix();
                
                var stuckOffset = Point.Zero;
                if (_iAmStuck)
                {
                    if (Position.X is < GameManager.MapWidth*32 and > 0 &&
                        Position.Y is < GameManager.MapHeight*32 and > 0)
                    {
                        stuckOffset = new(_random.Next(-1, 2), _random.Next(-1, 2));
                        if (cells[(int)Position.X / PositionScalingFactor + stuckOffset.X,
                                (int)Position.Y / PositionScalingFactor + stuckOffset.Y])
                            stuckOffset = Point.Zero;
                    } else stuckOffset = Point.Zero;
                }
                GameManager.ScreenManager.InGameScreen.Map.PathStart = Position;
                GameManager.ScreenManager.InGameScreen.Map.PathEnd = new Vector2(_positions[_positionIndex].Item1,
                    _positions[_positionIndex].Item2);
               
                PathFinder pathfinder = new PathFinder(GameManager.ScreenManager.InGameScreen.Map.MapWidth,
                    GameManager.ScreenManager.InGameScreen.Map.MapHeight, cells);
                var destiny = (_positions[_positionIndex].Item1, _positions[_positionIndex].Item2);
                if (_recentHit)
                {
                    var flightVector =
                        GameManager.ScreenManager.InGameScreen.GameObjectManager.SpawnManager.randomSpawnPosition(1200f);
                    _flightPosition = ((int)flightVector.X, (int)flightVector.Y);
                    destiny = _flightPosition;
                    _recentHit = false;
                }
                
                var newPath =
                    pathfinder.findPath(
                        (stuckOffset.X + (int)Position.X / PositionScalingFactor, stuckOffset.Y + (int)Position.Y / PositionScalingFactor),
                        destiny);

                if (newPath != null)
                {
                    _path = newPath;
                    _lastPath = newPath;
                    _pathIndex = 0; // Reset only if a new path is found
                }
                else
                {
                    // Keep following the last known path
                    _path = _lastPath;
                }
                _pathfindingCooldown = _random.Next(PathfindingIntervalLowerBound, PathfindingIntervalUpperBound);
                isPoliceInRange();
        }
        
        if (_path != null && _pathIndex < _path.Count)
        {   
            goToNextSpot(gameTime);
                
            var animatable = getComponent<Animatable>();
            if (_isRunning && getComponent<Dialogable>().IsClosed)
            {
                if (animatable.CurrentAnimationName != "running") animatable.startAnimation("running"); 
            }
            else if (!getComponent<Dialogable>().IsClosed)
            {
                if (animatable.CurrentAnimationName != "idle") animatable.startAnimation("idle");
            }
            else
            {
                if (animatable.CurrentAnimationName != "walking") animatable.startAnimation("walking");
            }
        }
    }

    public override void render(GameTime gameTime)
    {
        // currently selected animation that should be drawn
        var currentAnimation = getComponent<Animatable>().CurrentAnimation;

        // rectangle that represents the image on the SpriteSheet that is currently needed for animation
        var sourceRectangle = currentAnimation.CurrentRectangle;

        // offset to position of the rendered image, so that the actual position is in the middle of the feet
        var renderPositionOffSet = new Point(-(int)(sourceRectangle.Width * ImageScalingFactor) / 2,
            -((int)(sourceRectangle.Height * ImageScalingFactor)));
        // scales the size of the rendered image by ImageScalingFactor
        var scaledSize = new Point((int)Math.Round(sourceRectangle.Width * ImageScalingFactor),
            (int)Math.Round(sourceRectangle.Height * ImageScalingFactor));
        // rectangle the image should be drawn in
        var destinationRectangle = new Rectangle(Position.ToPoint() + renderPositionOffSet, scaledSize);

        // check if last direction was to left or right and based on that mirror the image
        var spriteEffect = _directionAnimation.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        float layerDepth = Position.Y / 100000;
        GameManager.SpriteBatch.Draw(currentAnimation.SpriteSheet, destinationRectangle, sourceRectangle, Color.White,
            0f, Vector2.Zero, spriteEffect, layerDepth);
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
        {
            getComponent<Collider>().drawHitBox();
            getComponent<Collider>().drawNeighboursInRadius(Radius);
        }
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawTileCollisionRects, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawTileCollisionHitBox();
    }

    public override void loadContent()
    {
        var animationIndex = _priceFactor switch
        {
            1f => 0,
            1.05f => 0,
            1.1f => 1,
            1.15f => 2,
            1.2f => 3,
            _ => 0
        };
        loadAnimations(_animations[animationIndex]);
    }

    private void loadAnimations(string filename)
    {
        const int rectangleWidth = 128;
        const int rectangleHeight = 128;
        var animatable = getComponent<Animatable>();

        animatable.addAnimation("idle", new Animation(GameManager.AssetManager.Images[filename + "_Idle"], 6, rectangleWidth, rectangleHeight, 150, 0, true));
        animatable.addAnimation("running", new Animation(GameManager.AssetManager.Images[filename + "_Run"], 10, rectangleWidth, rectangleHeight, 60, 0, true));
        animatable.addAnimation("walking", new Animation(GameManager.AssetManager.Images[filename + "_Walk"], 8, rectangleWidth, rectangleHeight, 60, 0, true));
        animatable.addAnimation("hurt", new Animation(GameManager.AssetManager.Images[filename + "_Hurt"], 4, rectangleWidth, rectangleHeight, 60, 2, false));
        animatable.addAnimation("die", new Animation(GameManager.AssetManager.Images[filename + "_Dead"], 5, rectangleWidth, rectangleHeight, 150, 3, false, true));
        
        animatable.CurrentAnimationName = "idle";
        animatable.CurrentAnimation.start();
    }

    public override void draw(GameTime gameTime)
    {
    }

    public override void saveState(GameState gameState)
    {
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.NpcBuyer, Position.X, Position.Y, Needs, _buyCooldown, _path, _pathIndex, _positions, _positionIndex, getComponent<InactiveCombatant>().Health, _priceFactor));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
        Position = new Vector2(gameObject.X, gameObject.Y);
        Needs = gameObject.Stock;
        _buyCooldown = gameObject.ReStockCoolDown;
        _path = gameObject.Path;
        _pathIndex = gameObject.PathIndex;
        _positions = gameObject.PatrolPoints;
        _positionIndex = gameObject.PatrolPointIndex;
        getComponent<InactiveCombatant>().Health = gameObject.Health;
        _priceFactor = gameObject.PriceFactor;
        if (gameObject.Health <= 0)
        {
            _flag = true;
            getComponent<InactiveCombatant>().receiveDamage(0);
        }
    }

    public Dictionary<ItemName, int> fetchInventory() => Needs;
    public List<(int, int)> getRoute() => _positions;
}