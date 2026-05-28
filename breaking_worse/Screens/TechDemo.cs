using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Input;
using breaking_worse.Input.Enums;
using breaking_worse.input.KeyTypes;
using breaking_worse.Objects.Enemy;
using breaking_worse.Objects.Interaction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vector2 = System.Numerics.Vector2;

namespace breaking_worse.Screens;

public class TechDemo
{
    public TechDemo(GameManager gameManager)
    {
        _gameManager = gameManager;
        _inputHandler = _gameManager.InputHandler;
    }
    private readonly GameManager _gameManager;
    private readonly InputHandler _inputHandler;
    
    // Direction of the Walts's field of view to spawn using keyboard
    private Microsoft.Xna.Framework.Vector2 _directionFov = new(1, 0);

    // Because map isn't in any constructor of InGameScreen
    private bool _getCollisionMatrixOnce; 
    private bool[,] _collisionCells;

    // Bool to choose if spawn multiple entities
    private readonly KeyboardKey _blockModeKey = new(Keys.B);
    private readonly KeyboardKey _spawnKey = new(Keys.T);
    
    // Mapping input keys to entity types
    private readonly Dictionary<IKey, Type> _possibleTypes = new()
    {
        { new KeyboardKey(Keys.G), typeof(Npc) },
        { new KeyboardKey(Keys.Y), typeof(Cartel) },
        { new KeyboardKey(Keys.H), typeof(Trash) },
        { new KeyboardKey(Keys.U), typeof(Police) },
        { new KeyboardKey(Keys.J), typeof(NpcBuyer) }
    };

    private Vector2 _mouse;
    private Vector2 _mouseMapPosition;

    private void spawnEntity(Type entityType, bool withMouse = false)
    {
        if (BlockMode)
        {
            spawnBlockEntity(entityType, withMouse);
            return;
        }
        spawnSingleEntity(entityType, withMouse);
    }

    // Spawns a single entity at a calculated position
    private void spawnSingleEntity(Type entityType, bool withMouse = false)
    {
        var position = withMouse 
            ? new Vector2(_mouseMapPosition.X * 32, _mouseMapPosition.Y * 32) 
            : new Vector2(
                _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position.X + (_directionFov.X) * 12 , 
                _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position.Y + (_directionFov.Y) * 12);
        
        if (_collisionCells[(int)position.X/32, (int)position.Y/32] ||
            !_gameManager.ScreenManager.InGameScreen.GameObjectManager.spawnEntity(entityType, position)) return;

        var (name, amount) = Tracker[entityType];
        Tracker[entityType] = (name, amount + 1);
    }

    // Spawns multiple entities in a block pattern
    private void spawnBlockEntity(Type entityType, bool withMouse = false)
    {
        var position = withMouse 
            ? new Vector2(_mouseMapPosition.X * 32, _mouseMapPosition.Y * 32) 
            : new Vector2(
                _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position.X,
                _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position.Y);
        var npcCount = 0;
        var random = new Random();
        
        var range = withMouse ? 5 : 12;
        var minDistance = withMouse ? 4 : 7;
        
        int startXClamp = Math.Clamp((int)position.X/32 - range, 0, _collisionCells.GetLength(0));
        int endXClamp = Math.Clamp((int)position.X/32 + range, 0, _collisionCells.GetLength(0));
        int startYClamp = Math.Clamp((int)position.Y/32 - range, 0, _collisionCells.GetLength(1));
        int endYClamp = Math.Clamp((int)position.Y/32 + range, 0, _collisionCells.GetLength(1));

        for (int x = startXClamp; x < endXClamp; x += 2)
        {
            for (int y = startYClamp; y < endYClamp; y += 2)
            {
                if (npcCount == 100) break;
                Vector2 spawnPosition = new Vector2(x * 32 + random.Next(-32, 32), y * 32 + random.Next(-32, 32));
                if (_collisionCells[(int)spawnPosition.X/32, (int)spawnPosition.Y/32]) continue;
                if (Vector2.Distance(new Vector2(_gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position.X,
                        _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position.Y),
                        spawnPosition) < minDistance * 32) continue;
                if (_gameManager.ScreenManager.InGameScreen.GameObjectManager.spawnEntity(entityType, spawnPosition))
                {
                    var (name, amount) = Tracker[entityType];
                    Tracker[entityType] = (name, amount + 1);
                }
                npcCount++;
            }
            if (npcCount == 100) break;
        }
        
    }

    public void update(GameTime gameTime)
    {
        if (_gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInteracting || 
            _gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInteracting ||
            _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInventoryOpen ||
            _gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInventoryOpen) return;
        
        _directionFov = new Microsoft.Xna.Framework.Vector2(
            _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.DirectionFov.X,
            _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.DirectionFov.Y);

        if (!_getCollisionMatrixOnce)
        {
            _collisionCells = _gameManager.ScreenManager.InGameScreen.Map.getCollisionMatrix();
            _getCollisionMatrixOnce = true;
        }
        
        foreach (var key in _possibleTypes.Keys.Where(key => _inputHandler.isPressed(key, PressType.PressAndRelease)))
        {
            LastSpawnType = _possibleTypes[key];
        }

        if (_inputHandler.isPressed(_blockModeKey, PressType.PressAndRelease))
        {
            BlockMode = !BlockMode;
        }
        
        if (_inputHandler.isPressed(_spawnKey, PressType.PressAndRelease))
        {
            spawnEntity(LastSpawnType);
        }

        if (_gameManager.UserActionHandler.isLeftMouseButtonClicked())
        {
                    
            _mouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            
            _mouseMapPosition = new Vector2(
                (int)((_mouse.X / _gameManager.ScreenManager.InGameScreen.Camera.Scale + _gameManager.ScreenManager.InGameScreen.Camera.Position.X)/32),
                (int)((_mouse.Y / _gameManager.ScreenManager.InGameScreen.Camera.Scale + _gameManager.ScreenManager.InGameScreen.Camera.Position.Y)/32)
            );
            
            spawnEntity(LastSpawnType, true);
        }
    }
    

    public bool BlockMode { get; set; }

    public Type LastSpawnType { get; set; } = typeof(Npc);

    public Dictionary<Type, (string name, int amount)> Tracker { get; } = new()
    {
        { typeof(Npc), ("NpcSeller:G", 0) },
        { typeof(NpcBuyer), ("NpcBuyer:J", 0) },
        { typeof(Cartel), ("Cartel:Y", 0) },
        { typeof(Trash), ("Trash:H", 0) },
        { typeof(Police), ("Police:U", 0) }
    };
}