using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Enemy;
using breaking_worse.Objects.Interaction;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.Screens;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace breaking_worse.Objects;

public class GameObjectManager
{
    /// <summary>
    /// A list that contains all the gameObjects and updates, renders, saves, ... them
    /// add or remove gameObjects to the list to create or remove them from the map
    /// </summary>
    private List<AGameObject> _gameObjects;
    private readonly List<AGameObject> _toBeRemoved = [];
    private Dictionary<AGameObject, double> _toBeReSpawned = [];
    private const double ReSpawnTime = 5000d;
    private Random _random;

    public int ObjectIdIndex;
    private readonly GameManager _gameManager;
    public PlayerCharacter Walt { get; private set; }
    public PlayerCharacter Jesse { get; private set; }
    public CookingStation CookingStation { get; private set; }
    
    public SpawnManager SpawnManager;
    
    public List<Pickables> Drops { get; set; } = [];
    public List<Pickables> ToBeRemovedPickables { get; set; } = [];
    
    public GameObjectManager(GameManager gameManager, InGameScreen inGameScreen)
    {
        _gameManager = gameManager;
        ObjectIdIndex = 0;
        _random = new Random();
        var multiplier = (int)_gameManager.SettingsManager.getDifficultySettings().NpcBuyMultiplier;
        Walt = new PlayerCharacter(_gameManager, PlayerCharacterId.Walt);
        Jesse = new PlayerCharacter(_gameManager, PlayerCharacterId.Jesse);
        CookingStation = new CookingStation(_gameManager, inGameScreen);
        var door = new Door(_gameManager);
        var house = new House(_gameManager, _gameManager.AssetManager.Images["WaltHouse"], new Vector2(142 * 32 - 3, 70 * 32 - 3), 0.565f);
        var hospital = new House(_gameManager, _gameManager.AssetManager.Images["Hospital"], new Vector2(97 * 32 - 24, 68 * 32 - 20), 0.575f);
        
        
        _gameObjects = [Walt, Jesse, CookingStation, door, house, hospital];
        foreach (var gameObject in _gameObjects)
        {
            gameObject.ObjectId = ObjectIdIndex++;
        }
        SpawnManager = new SpawnManager(gameManager, ObjectIdIndex);
        _gameObjects = SpawnManager.spawnNpc(_gameObjects);
        ObjectIdIndex = _gameObjects.Last().ObjectId + 1;
        
        createTrash(25 * multiplier);
        createBushes();
    }
    
    public void update(GameTime gameTime)
    {
        var wantedLevel = _gameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel;
        foreach (var gameObject in _gameObjects)
        {
            gameObject.update(gameTime);
            if (gameObject.GetType() == typeof(Trash))
            {
                var trash = gameObject as Trash;
                if (trash.Cooldown <= 0)
                {
                   _toBeRemoved.Add(trash);
                   trash.getComponent<Collider>().ExcludeFromCollisions = true;
                   createTrash(1);
                   break;
                }
            }
        }

        if (_gameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel > wantedLevel)
        {
            var li = SpawnManager.increasePolice(_gameObjects, wantedLevel);
            foreach (var gameObject in li)
            {
                gameObject.loadContent();
                addGameObject(gameObject);
            }
        }

        if (_gameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel < wantedLevel)
        {
            _gameObjects = SpawnManager.adjustPoliceToWantedLevel(_gameObjects, wantedLevel);
        }
        
        
        foreach (var gameObject in _toBeRemoved)
        {
            updateGameObjects(gameObject);
            var spawnObject = SpawnManager.reSpawnNpc(gameObject.Type, gameObject.ObjectId);
            if (spawnObject != null)
                _toBeReSpawned.Add(spawnObject, ReSpawnTime);
            if (gameObject.hasComponent<Collider>())
                _gameManager.ScreenManager.InGameScreen.CollisionManager.removeCollider(gameObject.getComponent<Collider>());
        }
        _toBeRemoved.Clear();
        
        var newRespawnList = new Dictionary<AGameObject, double>();
        foreach (var (spawnObject, time) in _toBeReSpawned)
        {
            if (time <= 0)
            {
                var npc = SpawnManager.replicateObject(spawnObject);
                npc.loadContent();
                npc.ObjectId = spawnObject.ObjectId;
                _gameObjects.Add(npc);
            }
            else
            {
                newRespawnList[spawnObject] = time - gameTime.ElapsedGameTime.TotalMilliseconds;
            }
        }
        _toBeReSpawned = newRespawnList;
        
        foreach (var drop in Drops)
        {
            drop.absorbDrops();
            if(drop.Drops.Count == 0) ToBeRemovedPickables.Add(drop);
        }
        
        foreach (var drop in ToBeRemovedPickables)
        {
            Drops.Remove(drop);
        }
        ToBeRemovedPickables.Clear();
    }
    
    public void render(GameTime gameTime)
    {
        foreach (var gameObject in _gameObjects)
        {
            gameObject.render(gameTime);
        }

        foreach (var drop in Drops)
        {
            drop.render(gameTime);
        }
    }

    public void addPickable(Pickables pickable)
    {
        Drops.Add(pickable);
    }

    public void removePickable(Pickables pickable)
    {
        ToBeRemovedPickables.Add(pickable);
    }

    private void createTrash(int amount)
    {
        // selelct every second sidewalk tiles and create random positions to spawn trash
        List<Vector2> sidewalkTiles = _gameManager.ScreenManager.InGameScreen.Map.getTilePositions(new List<int>{1099, 1101, 1103, 1258, 1260, 1262, 1264, 1122, 1186, 1250, 1314, 1095, 1159, 1223, 1287});
        var randomVectors = getRandomVectors(sidewalkTiles, amount);
        foreach (var position in randomVectors)
        {
            var trash = new Trash(_gameManager, position);
            addGameObject(trash);
        }
    }

    private void createBushes()
    {
        addGameObject(new Bush(_gameManager, new Vector2(15 * 32, 5 * 32)));
        addGameObject(new Bush(_gameManager, new Vector2(225 * 32, 107 * 32)));
        addGameObject(new Bush(_gameManager, new Vector2(128 * 32, 27 * 32)));
        addGameObject(new Bush(_gameManager, new Vector2(65 * 32, 133 * 32)));
        addGameObject(new Bush(_gameManager, new Vector2(164 * 32, 19 * 32)));
        addGameObject(new Bush(_gameManager, new Vector2(48 * 32, 55 * 32)));
        addGameObject(new  Car(_gameManager, new Vector2(133 * 32, 70 * 32)));
    }
    
    public List<AGameObject> getBushes()
    {
        return _gameObjects
            .Where(gameObject => gameObject is Bush)
            .ToList();
    }
    
    public void addGameObject(AGameObject gameObject)
    {
        _gameObjects.Add(gameObject);
        gameObject.ObjectId = ObjectIdIndex++;
    }
    
    public bool spawnEntity(Type objectType, Vector2 position)
    {
        AGameObject newGameObject;
        switch (objectType)
        {
            case not null when objectType == typeof(Cartel):
                newGameObject = SpawnManager.createAGameObject(NpcType.Cartel,position, ObjectIdIndex++, 
                    SpawnManager.getNewRoute(true));
                SpawnManager.InGameNpc[NpcType.Cartel].Add((newGameObject, ObjectIdIndex));
                break;

            case { } t when t == typeof(NpcBuyer):
                newGameObject = SpawnManager.createNpcBuyer(_gameManager,position, ObjectIdIndex++);
                SpawnManager.InGameNpc[NpcType.NpcBuyer].Add((newGameObject, ObjectIdIndex));
                break;

            case { } t when t == typeof(Police):
                newGameObject = SpawnManager.createAGameObject(NpcType.Police,position, ObjectIdIndex++, 
                    SpawnManager.getNewRoute(false));
                SpawnManager.InGameNpc[NpcType.Police].Add((newGameObject, ObjectIdIndex));
                break;
            
            case { } t when t == typeof(Trash):
                newGameObject = new Trash(_gameManager, position);
                break;
            
            case { } t when t == typeof(Bush):
                newGameObject = new Bush(_gameManager, position);
                break;
            
            case { } t when t == typeof(Npc):
                newGameObject = SpawnManager.createNpc(_gameManager, position, 0);
                break;

            default:
                return false;
        }
        
        newGameObject.loadContent();
        addGameObject(newGameObject);
        return true;
    }

    private void updateGameObjects(AGameObject gameObject)
    {
        var newGameObjects = _gameObjects.Where(iObject => iObject.ObjectId != gameObject.ObjectId).ToList();
        _gameObjects = newGameObjects;
    }
    
    public void removeGameObject(AGameObject gameObject)
    {
        _toBeRemoved.Add(gameObject);
    }
    
    public void loadState(GameState gameState)
    {
        foreach (var gameObject in _gameObjects.Where(gameObject => gameObject.getComponent<Collider>() is not null))
            _gameManager.ScreenManager.InGameScreen.CollisionManager.removeCollider(gameObject.getComponent<Collider>());
        
        _gameObjects.Clear();
        _toBeReSpawned.Clear();
        foreach (var gameObject in gameState.SavedGameObjects)
        {
            var isRespawn = gameState.ReSpawnTimes.ContainsKey(gameObject.ObjectId);
            AGameObject newGameObject = null;
            switch (gameObject.Type)
            {
                case NpcType.Walt:
                    Walt.loadState(gameObject);
                    _gameObjects.Add(Walt);
                    break;
                case NpcType.Jesse:
                    Jesse.loadState(gameObject);
                    _gameObjects.Add(Jesse);
                    break;
                case NpcType.Police:
                    newGameObject = new Police(_gameManager, new Vector2(gameObject.X, gameObject.Y));
                    break;
                case NpcType.Cartel:
                    newGameObject = new Cartel(_gameManager, new Vector2(gameObject.X, gameObject.Y));
                    break;
                case NpcType.NpcBuyer:
                    newGameObject = new NpcBuyer(_gameManager, new Vector2(gameObject.X, gameObject.Y), null, null, gameObject.PriceFactor);
                    break;
                case NpcType.NpcSeller:
                    newGameObject = new Npc(_gameManager, gameObject.Name,new Vector2(gameObject.X, gameObject.Y));
                    break;
                case NpcType.Trash:
                    newGameObject = new Trash(_gameManager, new Vector2(gameObject.X, gameObject.Y));
                    break;
                case NpcType.Bush:
                    newGameObject = new Bush(_gameManager, new Vector2(gameObject.X, gameObject.Y));
                    break;
                case NpcType.Car:
                    newGameObject = new Car(_gameManager, new Vector2(gameObject.X, gameObject.Y));
                    break;
                case NpcType.CookingStation:
                    CookingStation.loadState(gameObject);
                    _gameObjects.Add(CookingStation);
                    break;
                case NpcType.Door:
                    newGameObject = new Door(_gameManager);
                    break;
                case NpcType.House:
                    newGameObject = new House(_gameManager, _gameManager.AssetManager.Images[gameObject.Texture], new Vector2(gameObject.X, gameObject.Y), gameObject.Scale);
                    break;
            }
            
            if (newGameObject is null)
                continue;
            
            newGameObject.loadContent();
            newGameObject.loadState(gameObject);
            
            if (isRespawn)
                _toBeReSpawned.Add(newGameObject, gameState.ReSpawnTimes[gameObject.ObjectId]);
            else
                _gameObjects.Add(newGameObject);
        }

        foreach (var savedPickable in gameState.SavedPickables)
        {
            Drops.Add(new Pickables(_gameManager, new Vector2(savedPickable.PositionX, savedPickable.PositionY), savedPickable.Drops));
        }
    }

    public void draw(GameTime gameTime)
    {
        foreach (var gameObject in _gameObjects)
            gameObject.draw(gameTime);
        
    }
    
    public void saveState(GameState gameState)
    {
        gameState.SavedGameObjects = [];
        foreach (var gameObject in _gameObjects)
        {
            gameObject.saveState(gameState);
        }
        
        gameState.ReSpawnTimes = new Dictionary<int, double>();
        foreach (var reSpawnObject in _toBeReSpawned)
        {
            reSpawnObject.Key.saveState(gameState);
            gameState.ReSpawnTimes[reSpawnObject.Key.ObjectId] = reSpawnObject.Value;
        }

        gameState.SavedPickables = [];
        foreach (var drop in Drops)
        {
            gameState.SavedPickables.Add(new SavedPickable(drop.Position.X, drop.Position.Y, drop.Drops));
        }
    }
    
    public void loadContent()
    {
        foreach (var gameObject in _gameObjects)
        {
            gameObject.loadContent();
        }
    }
    
    public List<AGameObject> getObjectsThatHaveComponent<T>()
    { 
        return _gameObjects.Where(gameObject => gameObject.hasComponent<T>()).ToList();
    }
    
    // selects random a given amount of vectors out of a list of vectors
    private static List<Vector2> getRandomVectors(List<Vector2> vectors, int count)
    {
        Random random = new Random();
        for (int i = vectors.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (vectors[i], vectors[j]) = (vectors[j], vectors[i]);
        }
        return vectors.Take(count).ToList();
    }
}
