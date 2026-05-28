using System;
using System.Collections.Generic;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Combat;
using breaking_worse.Objects.Items;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Interaction;

public class Trash : AGameObject
{
    private readonly GameManager _gameManager;
    private readonly List<Item> _listItems = new();
    private Vector2 _position;
    private Texture2D _textureTrash;
    private readonly Random _random = new();
    private bool _dropItem;
    private bool _collectItem;
    private readonly int _randomNumber;
    private Item _item;
    public float Cooldown = 30.0f;

    public Trash(GameManager gameManager, Vector2 position): base(gameManager)
    {
        Type = NpcType.Trash;
        _gameManager = gameManager;
        
        _position = position;
        _textureTrash = _gameManager.AssetManager.Images["Trash"];
        
        //TODO: make extra list
        _listItems.Add(new Item(ItemName.Methylamine));
        _listItems.Add(new Item(ItemName.Phenylacetone));
        _listItems.Add(new Item(ItemName.Battery));
        _listItems.Add(new Item(ItemName.Petrol));
        _listItems.Add(new Item(ItemName.Matches));
        _listItems.Add(new Item(ItemName.AluminumFoil));
        _listItems.Add(new Item(ItemName.Acetone));
        _listItems.Add(new Item(ItemName.Ammonia));
        _listItems.Add(new Item(ItemName.CocainePlant));
        _listItems.Add(new Item(ItemName.ColdMedicine));
        _listItems.Add(new Item(ItemName.ErlenmeyerFlask));
        
        foreach (var item in _listItems)
            item.Texture = _gameManager.AssetManager.Images[item.Name.ToString()];
        
        _randomNumber = _random.Next(1, 3);
        _item = selectRandomItem();
        
        addComponent(new InactiveCombatant(gameManager){IsTrash = true});
        addComponent(new Collider(gameManager, () => _position,
             Vector2.Zero,
            getComponent<InactiveCombatant>()){IsTrash = true});
    }

    private Item selectRandomItem()
    {
        int index = _random.Next(_listItems.Count);
        return _listItems[index];
    }
    
    public override void update(GameTime gameTime)
    {
        var collider = getComponent<Collider>();
        collider.DynamicHitBox.Height = _textureTrash.Height;
        collider.DynamicHitBox.Width = _textureTrash.Width;
        
        // new Asset if Trash takes damage
        if (getComponent<InactiveCombatant>().Health == 1)
            _textureTrash = _gameManager.AssetManager.Images["Trash_halfBroken" + _randomNumber];
        
        if (getComponent<InactiveCombatant>().Health == 0)
        {
            _textureTrash = _gameManager.AssetManager.Images["Trash_broken" + _randomNumber];
            
            _dropItem = true;
            if (Cooldown > 0)
            {
                Cooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        // Collect Item
        var hitBoxItem = new Rectangle((int)(_position.X + _textureTrash.Width), (int)_position.Y, _item.Texture.Width, _item.Texture.Height);
        if ((hitBoxItem.Contains(_gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>().DynamicHitBox.CenterPoint)
             || hitBoxItem.Contains(_gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>().DynamicHitBox.CenterPoint))
            && !_collectItem)
        {
            _gameManager.ScreenManager.InGameScreen.PlayerState.Inventory.addAmount(_item, 1);
            
            _gameManager.ProgressManager.updateStatistics(Statistics.IngredientsCollected, 1, 1);
            _gameManager.ProgressManager.updateAchievements(Statistics.IngredientsCollected, Achievement.XIngredients);
            
            _collectItem = true;
        }
        
    }

    public override void render(GameTime gameTime)
    {
        var layerDepth = (_position.Y+ _textureTrash.Height) / 100000;
        GameManager.SpriteBatch.Draw(_textureTrash, _position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        if (_dropItem && !_collectItem)
            GameManager.SpriteBatch.Draw(_item.Texture, new Vector2(_position.X + _textureTrash.Width, _position.Y), Color.White);
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawHitBox();
    }

    public override void draw(GameTime gameTime) {}

    public override void saveState(GameState gameState)
    {
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.Trash, _position.X, _position.Y, getComponent<InactiveCombatant>().Health, Cooldown, _item.Name));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
        _position = new Vector2(gameObject.X, gameObject.Y);
        getComponent<InactiveCombatant>().Health = gameObject.Health;
        Cooldown = gameObject.ReStockCoolDown;
        _item = new Item(gameObject.ItemName);
        _item.Texture = _gameManager.AssetManager.Images[_item.Name.ToString()];
    }
    public override void loadContent() {}
}