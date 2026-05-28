using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Objects.Enemy;
using breaking_worse.Objects.Interaction;
using breaking_worse.Objects.Items;
using breaking_worse.State.Enums;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace breaking_worse.Objects;

public class SpawnManager
{
    private readonly GameManager _gameManager;
    
    public readonly Dictionary<NpcType, List<(AGameObject Object, int ID)>> InGameNpc = [];

    private const int SpawnRadius = 1000;
    
    private enum Territory
    {
        Home,
        Jail,
        Medium,
        Cartel,
        Rich
    }

    private Dictionary<ItemName, int> reStockNpc(string name)
    {
        var r = new Random();
        var inventory = new Dictionary<ItemName, int>();
        switch (name)
        {
            case "Doctor":
                inventory.Add(ItemName.ColdMedicine, 45);
                inventory.Add(ItemName.Hp, 15);
                inventory.Add(ItemName.CancerTreatment, 1);
                break;
            case "ShopChem":
                inventory.Add(ItemName.ErlenmeyerFlask, 15);
                inventory.Add(ItemName.Ammonia, 10);
                inventory.Add(ItemName.Acetone, r.Next(1, 10));
                inventory.Add(ItemName.AluminumFoil, r.Next(10, 30));
                break;
            case "ShopNormal":
                inventory.Add(ItemName.AluminumFoil, r.Next(10, 30));
                inventory.Add(ItemName.Matches, r.Next(50, 150));
                inventory.Add(ItemName.Battery, r.Next(10, 50));
                inventory.Add(ItemName.Petrol, r.Next(1, 10));
                break;
            case "PoliceStation":
                inventory.Add(ItemName.Ammo, r.Next(5, 50));
                break;
            default:
                inventory.Add(ItemName.Methylamine, r.Next(10));
                inventory.Add(ItemName.Phenylacetone, r.Next(10));
                inventory.Add(ItemName.CocainePlant, r.Next(10));
                break;
        }
        var newInventory = new Dictionary<ItemName, int>();
        if (inventory.Count == 0)
        {
            return [];
        }
        foreach (var itemPair in inventory)
        {
            if (itemPair.Value > 0)
            {
                newInventory.Add(itemPair.Key, itemPair.Value);
            }
        }
        return newInventory;
    }

    private Dictionary<ItemName, int> reStockNpcBuyer(Territory territory)
    {
        var inventory = new Dictionary<ItemName, int>();
        var r = new Random();
        switch (territory)
        {
            case Territory.Home:
                inventory.Add(ItemName.MethSmall, r.Next(1, 10));
                inventory.Add(ItemName.Meth,  r.Next(0, 10));
                inventory.Add(ItemName.Cocaine,  r.Next(0, 5));
                return inventory;
            case Territory.Jail:
                inventory.Add(ItemName.MethSmall, r.Next(1, 20));
                return inventory;
            case Territory.Medium:
                inventory.Add(ItemName.MethSmall, r.Next(1, 10));
                inventory.Add(ItemName.Meth,  r.Next(0, 10));
                inventory.Add(ItemName.Cocaine,  r.Next(0, 5));
                return inventory;
            case Territory.Cartel:
                inventory.Add(ItemName.Meth, r.Next(1, 10));
                inventory.Add(ItemName.Crack,  r.Next(1, 15));
                return inventory;
            case Territory.Rich:
                inventory.Add(ItemName.Cocaine,  r.Next(1, 20));
                return inventory;
            default:
                inventory.Add(ItemName.MethSmall, r.Next(1, 10));
                return inventory;
        }
    }

    private (int, int) newRandomPosition()
    {
        bool[,] cells = blockWaltHouseFromGeneration(_gameManager.ScreenManager.InGameScreen.Map.getCollisionMatrix());
        var r = new Random();
        var pos = (r.Next(1, GameManager.MapWidth), r.Next(1, GameManager.MapHeight));
        while (cells[pos.Item1, pos.Item2])
        {
            pos = (r.Next(1, GameManager.MapWidth), r.Next(1, GameManager.MapHeight));
        }
        return pos;
    }

    private Vector2 newRandomVector2()
    {
        var vector = newRandomPosition();
        return new Vector2(vector.Item1 * 32, vector.Item2 * 32);
    }

    private List<(int, int)> newRandomRoute()
    {
        List<(int, int)> route = [];
        var r = new Random();
        for (var i = 0; i < r.Next(6, 12); i++)
        {
            route.Add(newRandomPosition());
        }
        return route;
    }

    private bool[,] blockWaltHouseFromGeneration(bool[,] cells)
    {
        for (int i = 138; i < 160; i++)
        {
            for (int j = 63; j < 77; j++)
            {
                cells[i, j] = true;
            }
        }
        return cells;
    }

    public Vector2 randomSpawnPosition(float radius, int angle = -1)
    {
        var playerCenter = _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position
                           + ( _gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position
                               - _gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position) / 2;
        var r = new Random();
        bool[,] cells = blockWaltHouseFromGeneration(_gameManager.ScreenManager.InGameScreen.Map.getCollisionMatrix());
        
        var newPos = Vector2.Zero;
        var mapMax = new Vector2(cells.GetLength(0), cells.GetLength(1));
        
        while (cells[(int)(newPos.X / 32), (int)(newPos.Y / 32)])
        {
            var direction = angle < 0 ? r.Next(360) : r.Next(angle - 10 , angle + 10);
            newPos = new Vector2(playerCenter.X + (float)Math.Cos(direction * Math.PI / 180) * radius,
                playerCenter.Y + (float)Math.Sin(direction * Math.PI / 180) * radius);
            if (newPos.X < 0 || newPos.Y < 0 || newPos.X > mapMax.X * 32 || newPos.Y > mapMax.Y * 32)
                newPos = Vector2.Zero;
        }
        return newPos;
    }

    public Npc createNpc(GameManager gameManager, Vector2 position, int id, string name = "")
    {
        var territory = matchPositionToTerritory(new Vector2(position.X / 32f, position.Y / 32f));
        var priceFactor = 1f + 0.05f * (int)territory;
        
        var inventory = reStockNpc(name);
        if (inventory.Count == 0)
        {
            inventory.Add(ItemName.CancerTreatment, 1);
            priceFactor = 0.0001f;
        }
        name = name == "" ? territory switch
            {
                Territory.Jail => "Trader1",
                Territory.Medium => "Trader2",
                Territory.Cartel => "Trader1",
                Territory.Rich => "Trader2",
                _ => "Trader1"
            } : name;

        string text = name switch
        {
            "ShopChem" => "Welcome to Walgreens!",
            "ShopNormal" => "Welcome to Target!",
            "PoliceStation" or "Doctor" => "How can I help you?",
            _ => ""
        };
        var npc = new Npc(gameManager, name, position, inventory, text, priceFactor)
        {
            ObjectId = id
        };

        return npc;
    }

    public NpcBuyer createNpcBuyer(GameManager gameManager, Vector2 position, int id)
    {
        var territory = matchPositionToTerritory(new Vector2(position.X / 32f, position.Y / 32f));
        
        var priceFactor = 1f + 0.05f * (int)territory;
        
        var willBuy = reStockNpcBuyer(territory);
        
        var points = newRandomRoute();
        
        var npc = new NpcBuyer(gameManager, position, willBuy, points, priceFactor)
        {
            ObjectId = id
        };

        return npc;
    }

    public AGameObject createAGameObject(NpcType type, Vector2 position, int id, List<(int,int)> points = null)
    { 
        AGameObject npc = type == NpcType.Cartel
            ? new Cartel(_gameManager, position, points)
            : new Police(_gameManager, position, points);
        npc.ObjectId = id;
        return npc;
    }

    private static Territory matchPositionToTerritory(Vector2 position)
    {
        switch (position.Y)
        {
            case >= 107:
                return position.X < 90 ? Territory.Medium : Territory.Rich;
            case >= 96:
                if (position.X > 175) return Territory.Rich;
                return position.X < 74 ? Territory.Cartel : Territory.Medium;
            case >= 82:
                return position.X < 74 ? Territory.Cartel : Territory.Medium;
            case >= 59:
                if (position.X is >= 133 and <= 163) return Territory.Home;
                return position.X < 74 ? Territory.Cartel : Territory.Medium;
            case >= 35:
                return position.X < 74 ? Territory.Cartel : Territory.Medium;
            case < 35:
                return position.X < 102 ? Territory.Jail : Territory.Medium;
            default:
                return Territory.Medium;
        }
    }

    private List<(int, int)> getRoadPatrolRoute(int road)
    {
        List<(int,int)> route = road switch
        {
            // Police Routes
            1 => [(178, 128),(178, 100), (178, 79), (217, 79), (217, 88), (217, 95), (201, 95), (195, 107), (178, 108)],
            2 => [(218, 47), (178, 47), (178, 33), (178, 23), (178, 11), (207, 11), (178, 11), (178, 47)],
            3 => [(13, 117), (39, 117), (39, 129), (78, 129), (93, 129), (93, 121), (119, 121), (93, 121), (93, 93), (93, 117), (57, 117)],
            4 => [(118, 63), (118, 43), (118, 33), (123, 29), (123, 17), (117, 9), (133, 12), (123, 17), (123, 33), (134, 35), (135, 43),
                (133, 56), (133, 63)],
            // Cartel Path
            5 => [(39, 41), (56, 41), (59, 45), (66, 45), (59, 45), (59, 58), (48, 67), (74, 64), (74, 45), (59, 45), (56, 41)],
            6 => [(52, 107), (21, 107), (35, 107), (35, 92), (52, 92), (37, 92), (36, 80), (66, 80), (36, 80), (36, 107)],
            // No other Routes yet
            _ => null
        };
        return route;
    }

    private void createDefaultNpc(int index)
    {
        InGameNpc.Add(NpcType.Police, [ 
            // Patrol Officers: 1 per Route
            (createAGameObject(NpcType.Police, new Vector2(getRoadPatrolRoute(1)[0].Item1 * 32, getRoadPatrolRoute(1)[0].Item2 * 32), index++, getRoadPatrolRoute(1)), index), 
            (createAGameObject(NpcType.Police, new Vector2(getRoadPatrolRoute(2)[0].Item1 * 32, getRoadPatrolRoute(2)[0].Item2 * 32), index++, getRoadPatrolRoute(2)), index),
            (createAGameObject(NpcType.Police, new Vector2(getRoadPatrolRoute(3)[0].Item1 * 32, getRoadPatrolRoute(3)[0].Item2 * 32), index++, getRoadPatrolRoute(3)), index),
            (createAGameObject(NpcType.Police, new Vector2(getRoadPatrolRoute(4)[0].Item1 * 32, getRoadPatrolRoute(4)[0].Item2 * 32), index++, getRoadPatrolRoute(4)), index),
            (createAGameObject(NpcType.Police, newRandomVector2(), index++, newRandomRoute()), index), 
            (createAGameObject(NpcType.Police, newRandomVector2(), index++, newRandomRoute()), index), 
            (createAGameObject(NpcType.Police, newRandomVector2(), index++, newRandomRoute()), index), 
            (createAGameObject(NpcType.Police, newRandomVector2(), index++, newRandomRoute()), index), 
            
            // Police Station 1
            (createAGameObject(NpcType.Police, new Vector2(61 * 32, 32 * 32), index++), index),
            (createAGameObject(NpcType.Police, new Vector2(64 * 32, 32 * 32), index++), index),
            
            // Police Station 2
            (createAGameObject(NpcType.Police, new Vector2(186 * 32, 125 * 32), index++), index),
            (createAGameObject(NpcType.Police, new Vector2(188 * 32, 125 * 32), index++), index),

            // jail
            (createAGameObject(NpcType.Police, new Vector2(12 * 32, 22 * 32), index++), index),
            (createAGameObject(NpcType.Police, new Vector2(14 * 32, 22 * 32), index++), index)
            ]);
        InGameNpc.Add(NpcType.Cartel, [
            // Patrol Fighters
            (createAGameObject(NpcType.Cartel, new Vector2(getRoadPatrolRoute(5)[0].Item1 * 32, getRoadPatrolRoute(5)[0].Item2 * 32), index++, getRoadPatrolRoute(5)), index),
            (createAGameObject(NpcType.Cartel, new Vector2(getRoadPatrolRoute(6)[0].Item1 * 32, getRoadPatrolRoute(6)[0].Item2 * 32), index++, getRoadPatrolRoute(6)), index),
            (createAGameObject(NpcType.Cartel, newRandomVector2(), index++, newRandomRoute()), index),
            (createAGameObject(NpcType.Cartel, newRandomVector2(), index++, newRandomRoute()), index),
            
            // Idle Cartel Fighters
            (createAGameObject(NpcType.Cartel, new Vector2(67 * 32,48 * 32), index++), index),
            (createAGameObject(NpcType.Cartel, new Vector2(68 * 32, 50 * 32), index++), index),
            (createAGameObject(NpcType.Cartel, new Vector2(61 * 32, 58 * 32), index++), index),
            (createAGameObject(NpcType.Cartel, new Vector2(63 * 32, 58 * 32), index++), index),
            ]);
        InGameNpc.Add(NpcType.NpcSeller, [
            (createNpc(_gameManager, new Vector2(208 * 32, 123 * 32 - 16), index++, "ShopChem"), index),
            (createNpc(_gameManager, new Vector2(36 * 32, 112 * 32 - 16), index++, "ShopNormal"), index),
            
            (createNpc(_gameManager, new Vector2(187 * 32, 122 * 32 ), index++, "PoliceStation"), index),
            (createNpc(_gameManager, new Vector2(63 * 32, 27 * 32 ), index++, "PoliceStation"), index),
            
            (createNpc(_gameManager, new Vector2(109 * 32, 72 * 32), index++, "Doctor"), index),
            
            // Rich Seller
            (createNpc(_gameManager, new Vector2(140 * 32, 122 * 32), index++), index),
            
            // Cartel Seller
            (createNpc(_gameManager, new Vector2(26 * 32, 77 * 32), index++), index),
            
            // Medium Sellers 
            (createNpc(_gameManager, new Vector2(89 * 32, 60 * 32), index++), index),
            (createNpc(_gameManager, new Vector2(138 * 32, 99 * 32), index++), index),
            (createNpc(_gameManager, new Vector2(132 * 32, 58 * 32), index++), index),
            ]);
        InGameNpc.Add(NpcType.NpcBuyer, [
            // Jail
            (createNpcBuyer(_gameManager, new Vector2(27 * 32, 27 * 32), index++), index),
            
            // Cartel
            (createNpcBuyer(_gameManager, new Vector2(4 * 32, 45 * 32), index++), index),
            (createNpcBuyer(_gameManager, new Vector2(21 * 32, 92 * 32), index++), index),
            
            // Medium
            (createNpcBuyer(_gameManager, new Vector2(245 * 32, 11 * 32), index++), index),
            (createNpcBuyer(_gameManager, new Vector2(134 * 32, 16 * 32), index++), index),
            (createNpcBuyer(_gameManager, new Vector2(82 * 32, 137 * 32), index++), index),
            (createNpcBuyer(_gameManager, new Vector2(98 * 32, 78 * 32), index++), index),
            
            // Rich
            (createNpcBuyer(_gameManager, new Vector2(156 * 32, 137 * 32), index++), index),
            (createNpcBuyer(_gameManager, new Vector2(212 * 32, 110 * 32), index++), index)
            ]);
        for (int i = 0; i < 35; i++)
            InGameNpc[NpcType.NpcBuyer].Add((createNpcBuyer(_gameManager, newRandomVector2(), index + i), index + i));
    }

    public List<AGameObject> spawnNpc(List<AGameObject> gameObjects)
    {
        gameObjects.AddRange(from pair in InGameNpc from entry in pair.Value select entry.Object);
        return gameObjects;
    }

    public AGameObject replicateObject(AGameObject gameObject)
    {
        if (gameObject is Police police)
            return new Police(_gameManager, gameObject.Position, police.getRoute());
        else if (gameObject is Cartel cartel)
            return new Cartel(_gameManager, gameObject.Position, cartel.getRoute());
        else if (gameObject is NpcBuyer npcBuyer)
            return new NpcBuyer(_gameManager, gameObject.Position, npcBuyer.fetchInventory(), npcBuyer.getRoute(), 0.5f + 0.5f * (int)matchPositionToTerritory(new Vector2(gameObject.Position.X / 32f, gameObject.Position.Y / 32f)));
        else
            return null;
    }

    public List<(int, int)> getNewRoute(bool isCartel = false)
    {
        var r = new Random();
        var routes = isCartel ? (5, 7) : (1, 5);
        return getRoadPatrolRoute(r.Next(routes.Item1, routes.Item2));
    }

    public List<Police> increasePolice(List<AGameObject> gameObjects, int wantedLevel)
    {
        var polices = new List<Police>();
        for (var i = 0; i < 10 + wantedLevel * 4 - gameObjects.Count(obj => obj.Type == NpcType.Police); i++)
        {
            polices.Add(new Police(_gameManager, randomSpawnPosition(1000), getNewRoute()));
        }
        return polices;
    }
    
    
    public List<AGameObject> adjustPoliceToWantedLevel(List<AGameObject> gameObjects, int wantedLevel)
    {
        if (gameObjects.Count(obj => obj.Type == NpcType.Police) <= 10 + wantedLevel * 4)
            return gameObjects;
        while (gameObjects.Count(obj => obj.Type == NpcType.Police) > 10 + wantedLevel * 4)
        {
            var lastCop = gameObjects.Last(obj => obj.Type == NpcType.Police);
            gameObjects.Remove(lastCop);
        }
        return gameObjects;
    }

    public AGameObject reSpawnNpc(NpcType npcType, int objectId)
    {
        if (!InGameNpc.TryGetValue(npcType, out var value))
            return null;
        foreach (var (entry, id) in value)
        {
            if (objectId == id - 1)
            {
                entry.Position = randomSpawnPosition(SpawnRadius);
                return entry;
            }
        }
        return null;
    }
    
    public SpawnManager(GameManager gameManager, int objectIndex)
    {
        _gameManager = gameManager;
        createDefaultNpc(objectIndex);
    }
}