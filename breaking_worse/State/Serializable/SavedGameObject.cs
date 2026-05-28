using System.Collections.Generic;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.State.Enums;

namespace breaking_worse.State.Serializable;

public struct SavedGameObject
{
    public int ObjectId;
    public string Name;
    public string Text;

    public NpcType Type;
    public float X { get; set; }
    public float Y { get; set; }

    // Player Values
    
    public PlayerCharacterId playerCharacterId { get; set; }
    public bool IsHidden  { get; set; }
    public bool WaltInCar  { get; set; }
    public bool JesseInCar  { get; set; }
    public bool IsInteracting { get; set; }
    public bool IsInventoryOpen { get; set; }
    public Weapon EquippedWeapon { get; set; }
    public (float, float) DirectionFov;
    public (float, float) DirectionAnimation;
    
    // Enemy values
    public bool Patrolling { get; set; } = false;
    public bool IsAlerted { get; set; } = true;
    public List<(int, int)> Path;
    public int PathIndex;
    public List<(int, int)> PatrolPoints;
    public int PatrolPointIndex;
    
    // Friendly values
    public Dictionary<ItemName, int> Stock;
    public float ReStockCoolDown { get; set; }
    
    public float PriceFactor  { get; set; }
    
    // miscellaneous
    public int UpgradeLevel { get; set; }
    public string Texture { get; set; }
    public float Scale { get; set; }
    public bool IsOpen { get; set; }
    
    public int Health { get; set; }
    
    public ItemName ItemName { get; set; }
    

    private void setBasics(int id, NpcType npcType, float x, float y)
    {
        ObjectId = id;
        Type = npcType;
        X = x;
        Y = y;
    }
    
    public SavedGameObject(int id, NpcType npcType, float x, float y)
    {
        setBasics(id, npcType, x, y);
    }

    public SavedGameObject(int id, NpcType npcType, float x, float y,
        PlayerCharacterId playerCharacterIdNew, bool isHidden, bool isInteracting, bool isInventoryOpen, Weapon weapon,
        (float, float) directionFov, (float, float) directionAnimation)
    {
        setBasics(id, npcType, x, y);
        playerCharacterId = playerCharacterIdNew;
        IsHidden = isHidden;
        IsInteracting = isInteracting;
        IsInventoryOpen = isInventoryOpen;
        EquippedWeapon = weapon;
        DirectionFov = directionFov;
        DirectionAnimation = directionAnimation;
    }

    public SavedGameObject(int id, NpcType npcType, float x, float y,
        bool patrolling, bool isAlerted, List<(int, int)> path, int pathIndex,
        List<(int, int)> patrolPoints, int patrolPointIndex, (float,float) target, int health)
    {
        setBasics(id, npcType, x, y);
        Patrolling = patrolling;
        IsAlerted = isAlerted;
        Path = path;
        PathIndex = pathIndex;
        PatrolPoints = patrolPoints;
        PatrolPointIndex = patrolPointIndex;
        DirectionFov = target;
        Health = health;
    }
    
    public SavedGameObject(int id, NpcType npcType, float x, float y,
        Dictionary<ItemName, int> stock, int reStockCooldown, float priceFactor, string name = "", string text = "")
    {
        setBasics(id, npcType, x, y);
        Stock = stock;
        ReStockCoolDown = reStockCooldown;
        Name = name;
        Text = text;
        PriceFactor = priceFactor;
    }
    
    public SavedGameObject(int id, NpcType npcType, float x, float y,
        Dictionary<ItemName, int> stock, double reStockCooldown, 
        List<(int, int)> path, int pathIndex, List<(int, int)> patrolPoints, int patrolPointIndex, 
        int health, float priceFactor)
    {
        setBasics(id, npcType, x, y);
        Stock = stock;
        ReStockCoolDown = (float)reStockCooldown;
        Path = path;
        PathIndex = pathIndex;
        PatrolPoints = patrolPoints;
        PatrolPointIndex = patrolPointIndex;
        Health = health;
        PriceFactor = priceFactor;
    }
    
    public SavedGameObject(int id, NpcType npcType, float x, float y,
        int upgradeLevel)
    {
        setBasics(id, npcType, x, y);
        UpgradeLevel = upgradeLevel;
    }

    public SavedGameObject(int id, NpcType npcType, float x, float y,
        string texture, float scale = 1f)
    {
        setBasics(id, npcType, x, y);
        Texture = texture;
        Scale = scale;
    }
    
    public SavedGameObject(int id, NpcType npcType, float x, float y,
        string texture, bool waltInCar, bool jesseInCar, float scale = 1f)
    {
        setBasics(id, npcType, x, y);
        Texture = texture;
        Scale = scale;
        WaltInCar = waltInCar;
        JesseInCar = jesseInCar;
    }

    public SavedGameObject(int id, NpcType npcType, float x, float y, bool isOpen)
    {
        setBasics(id, npcType, x, y);
        IsOpen = isOpen;
    }
    
    public SavedGameObject(int id, NpcType npcType, float x, float y, int health, float coolDown, ItemName item)
    {
        setBasics(id, npcType, x, y);
        Health = health;
        ReStockCoolDown = coolDown;
        ItemName = item;
    }
    
}