using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Objects.Animations;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Combat;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using breaking_worse.Utility;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Enemy;

public class Police : AEnemy
{
    private (int x, int y) _previousCell;
    private int _alertRadius = 400;
    private readonly float _movementSpeedChase = 100f;
    private float _movementSpeedPatrolling = 50f;
    private bool _notMoving;
    private Pickables _pickable;
    private bool _flag;
    private readonly Random _random;
    private const float ChaseTime = 10000f;
    private const float ChaseCoolDown = 20000f;
    private float _chaseCoolDownTime;
    private float _currentChaseTime;
    private Vector2 _target;
    private bool _isChasing;
    private Vector2 _movingAverageDirection;
    
    private Vector2 _direction;
    private double _lastAttackTime;
    private readonly float _attackCooldown = 1f; 

    public Police(GameManager gameManager, Vector2 position, List<(int,int)> patrolPoints = null) : base(gameManager, position)
    {
        Type = NpcType.Police;
        _previousCell = ((int)Position.X / PositionScalingFactor, (int)Position.Y / PositionScalingFactor);
        IsAlerted = false;
        getComponent<Collider>().IsPolice = true;
        _random = new Random();
        
        _chaseCoolDownTime = 2500f; // 2.5s delay at start before cops start chasing
        _currentChaseTime = ChaseTime;
        
        populatePatrolPath(patrolPoints);
        IsPatrolling = !IsChilling;
    }

    public sealed override void populatePatrolPath(List<(int, int)> path = null)
    {
        if (path == null)
        {
            PatrolPoints = [((int)(Position.X / 32f), (int)(Position.Y / 32f))];
            IsChilling = true;
        }
        else PatrolPoints = path;
    }

    private void searchForNpc(GameTime gameTime)
    {
        // catch not currently chasing
        if (_chaseCoolDownTime > 0)
        {
            _currentChaseTime = ChaseTime;
            _movementSpeedPatrolling = 50f;
            _isChasing = false;
            _chaseCoolDownTime -= gameTime.ElapsedGameTime.Milliseconds;
            return;
        }
        
        // below is currently chasing
        
        // catch chase over
        if (_currentChaseTime <= 0)
        {
            _chaseCoolDownTime = ChaseCoolDown;
            return;
        }
        
        // search for NPC in Radius
        var npcList = GameManager.ScreenManager.InGameScreen.CollisionManager.getNeighborsInRadius(getComponent<Collider>(), 5).Where(npc => npc.IsNpc).ToList();
        var r = new Random();
        if (npcList.Count <= 0)
        {
            // Found no NPC to chase
            _movementSpeedPatrolling = 50f;
            return;
        } 
        // Found NPC to chase
        var randomIndex = r.Next(npcList.Count - 1);
        _target = new Vector2(npcList[randomIndex].DynamicHitBox.Position.X, npcList[randomIndex].DynamicHitBox.Position.Y);
        _movementSpeedPatrolling = 80f;
        _isChasing = true;
        _currentChaseTime -= gameTime.ElapsedGameTime.Milliseconds;
    }
    
    public override void chase(GameTime gameTime)
    {
        // Get the next target position from the path
        if (Path[PathIndex] == _previousCell)
        {
            PathIndex = (PathIndex + 1) % Path.Count;
        }
        if (!IsAlerted && !_isChasing && PatrolPoints.Count > 1 && PatrolPoints[PatrolPointIndex] == _previousCell)
        {
            PatrolPointIndex = (PatrolPointIndex + 1) % PatrolPoints.Count;
        }
        if (!IsAlerted && !_isChasing && PatrolPoints.Count == 1 && PatrolPoints[0] == _previousCell)
        {
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
                PathIndex = (PathIndex + 1) % Path.Count;
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
        _notMoving = Math.Abs(directionBeforeCollider.angle(_direction)) > Math.PI / 2f;
        // control facing of character sprite
        _movingAverageDirection += (_direction - _movingAverageDirection)/100f;
        DirectionAnimation = _movingAverageDirection;
        Position += _direction;

        // Clamp the position to avoid overshooting the target
        if (Vector2.Distance(Position, destination) < speed * deltaTime)
            Position = destination;

        _previousCell = ((int)Position.X / PositionScalingFactor, (int)Position.Y / PositionScalingFactor);
    }

    private void generateDrops()
    {
        int bullets = _random.Next(0, 10);
        if (bullets == 0) return;
        List<(PossiblePickables, int)> drops = [(PossiblePickables.Bullet, bullets)];
        _pickable = new Pickables(GameManager, Position, drops);
        GameManager.ScreenManager.InGameScreen.GameObjectManager.addPickable(_pickable);
    }

    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        if (getComponent<ActiveCombatant>().Health <= 0)
        {
            if (!_flag)
            {
                generateDrops();
                _flag = true;
            }
            return;
        }

        PathfindingCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        float distanceToWalt = Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position, Position);
        float distanceToJesse = Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position, Position);
        
        var wantedLevel = GameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel;
        _alertRadius = wantedLevel * 400;
        var currentTime = gameTime.TotalGameTime.TotalSeconds;
        var randomDelay = _random.Next(100, 2000) / 1000f;
        switch (wantedLevel)
        {
            case >= 1 and <= 3:
            {
                EquippedWeapon = Weapon.Fist;
                if (GameManager.ScreenManager.InGameScreen.PlayerState.Health < 5) break;
                //if wanted lvl between 1-3 and walt/jesse are in range + have more than 5 health, police will taser
                if ((!GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsHidden && getComponent<Collider>()
                        .isInRadius(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>(), 1)) 
                        || (!GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsHidden && getComponent<Collider>().isInRadius(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>(),
                            1)))
                {
                    //cooldown of 2 seconds for each attack
                    if (currentTime >= _attackCooldown + _lastAttackTime + randomDelay)
                    {
                        getComponent<ActiveCombatant>().attack(getComponent<Collider>(), _direction);
                        _lastAttackTime = currentTime;
                    }
                }
                break;
            }
            //if wanted lvl 4 or higher, police will shoot with a cooldown of 2 seconds
            case >= 4:
            {
                EquippedWeapon = Weapon.Gun;
                if (currentTime >= _attackCooldown + _lastAttackTime + randomDelay)
                {
                    getComponent<ActiveCombatant>().attack(getComponent<Collider>(), _direction);
                    _lastAttackTime = currentTime;
                }

                break;
            }
        }
        IsAlerted = (wantedLevel > 0 && distanceToJesse < _alertRadius * _alertRadius) 
                    || (wantedLevel > 0 && distanceToWalt < _alertRadius * _alertRadius);
        if (IsAlerted)
            IsChilling = IsPatrolling = false;
        if (PathfindingCooldown <= 0 || (Path != null && PathIndex >= Path.Count - 1))
        {
            Vector2 position = new Vector2();

            bool waltHidden = GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsHidden;
            bool jesseHidden = GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsHidden;

            if (waltHidden && jesseHidden) IsAlerted = false;
            else if (jesseHidden) position = GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position;
            else if (waltHidden) position = GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position;
            else
            {
                position = distanceToWalt <= distanceToJesse 
                    ? GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position 
                    : GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position;
            }
            
            if (!IsAlerted && !IsChilling)
            {
                searchForNpc(gameTime);
                IsPatrolling = !_isChasing;
                if (_isChasing)
                    position = _target;
            }
            findPath(position, _notMoving);
        }
        if (Path != null && PathIndex < Path.Count)
        {
            chase(gameTime);
            
            var animatable = getComponent<Animatable>();
            if (IsAlerted || _isChasing)
            {
                if (animatable.CurrentAnimationName != "running") animatable.startAnimation("running");
            }
            else if (IsPatrolling)
            {
                if (animatable.CurrentAnimationName != "Police_walk") animatable.startAnimation("Police_walk");
            }
            else
            {
                if (animatable.CurrentAnimationName != "idle") animatable.startAnimation("idle");
            }
        }
    }
    
    public override void loadContent()
    {
        loadContent("Police");
    }
    
    public override void draw(GameTime gameTime) {}

    public override void saveState(GameState gameState)
    {
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.Police, Position.X, Position.Y, IsPatrolling, IsAlerted, Path, PathIndex, PatrolPoints, PatrolPointIndex, (_target.X, _target.Y), getComponent<ActiveCombatant>().Health));
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
        _target = new Vector2(gameObject.DirectionFov.Item1, gameObject.DirectionFov.Item2);
        _isChasing = _target == Vector2.Zero;
        getComponent<ActiveCombatant>().Health = gameObject.Health;
        if (gameObject.Health <= 0)
        {
            _flag = true;
            getComponent<ActiveCombatant>().receiveDamage(0);
        }
    }
}
