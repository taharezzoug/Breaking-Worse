using System.Collections.Generic;
using breaking_worse.Objects.Collisions;
using breaking_worse.State.Enums;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Items;

public class Pickables
{
    
    private readonly GameManager _gameManager;
    private readonly Vector2 _position;

    private readonly List<Drop> _drops = [];
    private readonly List<Drop> _toBeRemoved = [];

    public Pickables(GameManager gamemanager, Vector2 position, List<(PossiblePickables possibleDrop, int quantity)> drops )
    {
        _gameManager = gamemanager;
        
        _position = position;
        initDrops(drops);
    }

    private void initDrops(List<(PossiblePickables possibleDrop, int quantity)> drops)
    {
        bool[,] grid = _gameManager.ScreenManager.InGameScreen.Map.getCollisionMatrix();
        
        var positionOnGridX = (int)_position.X/32;
        var positionOnGridY = (int)_position.Y/32;
        
        // Centers the drop position to the entity center
        var dropPositionX = (int)_position.X - 32;
        var dropPositionY = (int)_position.Y - 32;
        
        Rectangle dropHitbox;

        if (grid[positionOnGridX, positionOnGridY - 1] == false)
        {
            dropHitbox = new Rectangle(dropPositionX, dropPositionY - 32, 32, 32);
            
        } else if (grid[positionOnGridX, positionOnGridY + 1] == false)
        {
            dropHitbox = new Rectangle(dropPositionX, dropPositionY + 32, 32, 32);
            
        } else if (grid[positionOnGridX + 1, positionOnGridY] == false)
        {
            dropHitbox = new Rectangle(dropPositionX + 32, dropPositionY, 32 ,32);
            
        } else if (grid[positionOnGridX - 1, positionOnGridY] == false)
        {
            dropHitbox = new Rectangle(dropPositionX - 32, dropPositionY, 32 ,32);
        }
        else
        {
            dropHitbox = new Rectangle(dropPositionX, dropPositionY, 32, 32);
        }
        
        foreach (var (possibleDrop, quantity) in drops)
        {
            _drops.Add(new Drop(possibleDrop, quantity, dropHitbox));
            dropHitbox.X += 5;
            dropHitbox.Y += 5;
        }
    }

    public void render(GameTime gameTime)
    {
        foreach (var item in _drops)
        {
            var icon = _gameManager.AssetManager.Images[item.Pickable.ToString()];
            _gameManager.SpriteBatch.Draw(icon, item.DropHitbox, Color.White);
        }
    }
    
    
    // Method to check if the player has absorbed any of the dropped items (money, bullets, drugs)
    public void absorbDrops()
    {
        foreach (var item in _drops)
        {
            if (item.DropHitbox.Contains(_gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt
                    .getComponent<Collider>().DynamicHitBox.CenterPoint)
                || item.DropHitbox.Contains(_gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse
                    .getComponent<Collider>().DynamicHitBox.CenterPoint))
            {
                processAbsorb(item.Pickable, item.Quantity);
                _toBeRemoved.Add(item);
            }
        }

        foreach (var item in _toBeRemoved)
        {
            _drops.Remove(item);
        }
    
        _toBeRemoved.Clear();
    }


    // Method to handle the absorption of items (money, bullets, or drugs) by the player
    private void processAbsorb(PossiblePickables typeOfPickable, int quantity)
    {
        switch (typeOfPickable)
        {
            case PossiblePickables.Money:
                _gameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money += quantity;
                _gameManager.ProgressManager.updateStatistics(Statistics.ReceivedMoney, quantity, 1);
                break;
            case PossiblePickables.Bullet:
                _gameManager.ScreenManager.InGameScreen.PlayerState.Inventory.addAmount(new Item(ItemName.Ammo), quantity);
                break;
            case PossiblePickables.Cocaine:
                _gameManager.ScreenManager.InGameScreen.PlayerState.Inventory.addAmount(new Item(ItemName.Cocaine), quantity);
                break;
            case PossiblePickables.Crack:
                _gameManager.ScreenManager.InGameScreen.PlayerState.Inventory.addAmount(new Item(ItemName.Crack), quantity);
                break;
            case PossiblePickables.MethSmall:
                _gameManager.ScreenManager.InGameScreen.PlayerState.Inventory.addAmount(new Item(ItemName.MethSmall), quantity);
                break;
            case PossiblePickables.Meth:
                _gameManager.ScreenManager.InGameScreen.PlayerState.Inventory.addAmount(new Item(ItemName.Meth), quantity);
                break;
            case PossiblePickables.Matches:
                _gameManager.ScreenManager.InGameScreen.PlayerState.Inventory.addAmount(new Item(ItemName.Matches), quantity);
                break;
        }
    }

    public List<Drop> Drops => _drops;
    public Vector2 Position => _position;
}

public enum PossiblePickables{
    Money,
    Bullet,
    Crack,
    MethSmall,
    Cocaine,
    Meth,
    Matches
}

public class Drop(PossiblePickables pickable, int quantity, Rectangle dropHitbox)
{
    public PossiblePickables Pickable { get; } = pickable;
    public int Quantity { get; } = quantity;
    public Rectangle DropHitbox { get; set; } = dropHitbox; // Keep it mutable
}