using System;
using System.Collections.Generic;
using breaking_worse.Objects.Animations;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Combat;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Enemy;

public class Cartel : AEnemy
{
    private (int x, int y) _previousCell;
    private readonly float _movementSpeedChase = 100f;
    private readonly float _movementSpeedPatrolling = 50f;
    private int _flag;
    private bool _notMoving;
    private bool _willHit;
    private double _lastAttackTime;
    private float _attackCooldown = 1f;
    private readonly Random _random = new Random();
    private Vector2 _direction;
    private Pickables _pickable;
    private Rectangle _territory = new Rectangle(new Point(0, 35 * 32), new Point(74 * 32, 73 * 32));
    private Vector2 _movingAverageDirection;
    
    public override void populatePatrolPath(List<(int, int)> path = null)
    {
        if (path == null)
        {
            PatrolPoints = [((int)(Position.X / 32f), (int)(Position.Y / 32f))];
            IsChilling = true;
        }
        else PatrolPoints = path;
    }
    

    public Cartel(GameManager gameManager, Vector2 position, List<(int,int)> patrolPoints = null) : base(gameManager, position)
    {
        Type = NpcType.Cartel;
        _previousCell = ((int)Position.X / PositionScalingFactor, (int)Position.Y / PositionScalingFactor);
        IsAlerted = false;
        getComponent<Collider>().IsCartel = true;
        populatePatrolPath(patrolPoints);
        IsPatrolling = !IsChilling;
    }
    
    private void generateDrops()
    {
        int money = _random.Next(20, 200);
        int bullets = _random.Next(1, 5);
        bool dropCoke = _random.Next(0, 4) == 0;
        
        List<(PossiblePickables, int)> drops = [(PossiblePickables.Money, money), (PossiblePickables.Bullet, bullets)];
        if(dropCoke) drops.Add((PossiblePickables.Cocaine, 1)); 
        
        _pickable = new Pickables(GameManager, Position, drops);
        GameManager.ScreenManager.InGameScreen.GameObjectManager.addPickable(_pickable);
    }

    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        
        if (_flag == -1) return;
        if (getComponent<ActiveCombatant>().Health <= 0 && _flag == 0)
        {
            _flag = 1;
            return;
        }
        if (getComponent<ActiveCombatant>().Health <= 0 && _flag == 1)
        {
            GameManager.ProgressManager.updateStatistics(Statistics.CartelKilled, 1, 1);
            GameManager.ProgressManager.updateAchievements(Statistics.CartelKilled, Achievement.KillXCartel);
            _flag = -1;
            generateDrops();
            return;
        }
        updateWillHit(gameTime);
        updateWillShoot(gameTime);
        var currentTime = gameTime.TotalGameTime.TotalSeconds;
        var randomDelay = _random.Next(100, 2000) / 1000f;
        if (_willHit)
        {
            EquippedWeapon = Weapon.Fist;
            if (getComponent<Collider>()
                    .isInRadius(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>(), 1)
                || getComponent<Collider>().isInRadius(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>(),
                    1))
            {
                //cooldown of 2 seconds for each attack
                if (currentTime >= _attackCooldown + _lastAttackTime + randomDelay)
                {
                    getComponent<ActiveCombatant>().attack(getComponent<Collider>(), _direction);
                    _lastAttackTime = currentTime;
                }
            }
        }
        float distanceToWalt = Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position, Position);
        float distanceToJesse = Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position, Position);
        if (GameManager.ScreenManager.InGameScreen.PlayerState.WillShoot)
        {
            IsAlerted = true;
            IsPatrolling = false;
            var position = distanceToWalt <= distanceToJesse
                ? GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position 
                : GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position;
            findPath(position, _notMoving);
            if (Path != null && PathIndex < Path.Count)
                chase(gameTime);
            _willHit = false;
            EquippedWeapon = Weapon.Gun;
            if (currentTime >= _attackCooldown + _lastAttackTime + randomDelay)
            {
                getComponent<ActiveCombatant>().attack(getComponent<Collider>(), _direction);
                _lastAttackTime = currentTime;
            }
        }
        PathfindingCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        IsAlerted = _territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position)
                 || _territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position);
        IsPatrolling = !IsAlerted;
        if (IsAlerted)
            IsChilling = false;
        
        if (PathfindingCooldown <= 0)
        {
            var bothInTerritory =
                   _territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position) 
                && _territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position);
            
            var position = bothInTerritory && distanceToWalt <= distanceToJesse
                ? GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position 
                : GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position;
            
            if (!bothInTerritory)
                position = _territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position)
                    ? GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position 
                    : GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position;
            
            findPath(position, _notMoving);
        }
        
        
        if (Path != null && PathIndex < Path.Count)
        {
            chase(gameTime);
            
            var animatable = getComponent<Animatable>();
            if (IsAlerted && (distanceToJesse >= 1200 ||
                              distanceToWalt >= 1200))
            {
                if (animatable.CurrentAnimationName != "running") animatable.startAnimation("running");
            }
            else if (IsPatrolling)
            {
                if (animatable.CurrentAnimationName != "Cartel_walk") animatable.startAnimation("Cartel_walk");
            }
            else
            {
                if (animatable.CurrentAnimationName != "idle") animatable.startAnimation("idle");
            }
        }
    }

    private void updateWillHit(GameTime gameTime)
    {
        if (_territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position) &&
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInteracting
            || _territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position) &&
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInteracting)
            _willHit = true;
        if (!_territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position)
            && !_territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position))
            _willHit = false;
        
    }
    private void updateWillShoot(GameTime gameTime)
    {
        if (!_territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position)
            && !_territory.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position) && GameManager.ScreenManager.InGameScreen.PlayerState.WillShoot)
        {
            GameManager.ScreenManager.InGameScreen.PlayerState.AggroCooldown -= (float) gameTime.ElapsedGameTime.TotalSeconds;
        }
        if (GameManager.ScreenManager.InGameScreen.PlayerState.AggroCooldown <= 0) GameManager.ScreenManager.InGameScreen.PlayerState.WillShoot = false;
    }
    public override void loadContent()
    {
        loadContent("Cartel1");
    }

    public override void chase(GameTime gameTime)
    {
        // Get the next target position from the path
        if (Path[PathIndex] == _previousCell)
        {
            PathIndex++;
        }
        
        if (PatrolPoints.Count > 1 && PatrolPoints[PatrolPointIndex] == _previousCell)
        {
            PatrolPointIndex = (PatrolPointIndex + 1) % PatrolPoints.Count;
        }

        if (PatrolPoints.Count == 1 && PatrolPoints[0] == _previousCell)
        {
            IsChilling = true;
            IsPatrolling = false;
        }

        // Check if there's a path and if we haven't reached the end of it
        if (Path != null && PathIndex < Path.Count)
        {
            (int x, int y) nextCell = Path[PathIndex];
            Vector2 nextPosition = new Vector2(nextCell.x * PositionScalingFactor + PositionScalingFactor/2f, nextCell.y * PositionScalingFactor + PositionScalingFactor/2f);
            
            move(gameTime, nextPosition);

            // Check if the position has reached the destination
            if (Vector2.Distance(Position, nextPosition) < 10f)
            {
                PathIndex++;
            }
        }
    }

    public override void move(GameTime gameTime, Vector2 destination)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _direction = destination - Position;
        
        if (_direction.LengthSquared() > 0.0001f)
            _direction.Normalize();
        
        
        var speed = IsPatrolling ? _movementSpeedPatrolling : _movementSpeedChase;
        
        _direction = _direction * speed * deltaTime;
        
        var directionBeforeCollider = _direction;
        _direction = getComponent<Collider>().scaleDirectionByCollisions(_direction, gameTime, false);
        _notMoving = _direction.Length() < 0.5f * directionBeforeCollider.Length();
        
        // control facing of character sprite
        _movingAverageDirection += (_direction - _movingAverageDirection)/100f;
        DirectionAnimation = _movingAverageDirection;
        
        Position += _direction;

        // Clamp the position to avoid overshooting the target
        if (Vector2.Distance(Position, destination) < speed * deltaTime) 
            Position = destination;

        _previousCell = ((int)Position.X / PositionScalingFactor, (int)Position.Y / PositionScalingFactor);
    }
    public override void draw(GameTime gameTime) {}

    public override void saveState(GameState gameState)
    {
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.Cartel, Position.X, Position.Y, IsPatrolling, IsAlerted, Path, PathIndex, PatrolPoints, PatrolPointIndex, (0f,0f), getComponent<ActiveCombatant>().Health));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
        Position = new Vector2(gameObject.X, gameObject.Y);
        IsPatrolling = gameObject.Patrolling;
        IsAlerted = gameObject.IsAlerted;
        Path = gameObject.Path;
        PathIndex = gameObject.PathIndex;
        PatrolPoints = gameObject.PatrolPoints;
        PatrolPointIndex = gameObject.PatrolPointIndex;
        getComponent<ActiveCombatant>().Health = gameObject.Health;
        if (gameObject.Health <= 0)
        {
            _flag = -1;
            getComponent<ActiveCombatant>().receiveDamage(0);
        }
    }
}
