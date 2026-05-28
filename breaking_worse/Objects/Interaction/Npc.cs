using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Animations;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.ScreenTypes.InGameScreens;
using breaking_worse.Sound;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Interaction;

public class Npc : AGameObject
{
    private const float ImageScalingFactor = 1f;

    // HitBox dimensions
    private int DynamicHitBoxWidth = 20;
    private int DynamicHitBoxHeight = 20;
    // interaction radius
    private int Radius = 2;

    private double _cooldown;
    private float _priceFactor;
    private int _restockTimer;
    public Dictionary<ItemName, int> Stock { get; set; }= new();
    private string _textSeller;
    private int _originalScreenWidth;
    private float _scale;
    private String _name;
    private Dictionary<ItemName, int> _items;

    public Npc(GameManager gameManager, String name, Vector2 position, Dictionary<ItemName, int> items = null, String textSeller = "", float areaFactor = 1) : base(gameManager)
    {
        Type = NpcType.NpcSeller;
        Position = position;
        _name = name;
        _items = items;
        _restockTimer = (int)(30 * GameManager.SettingsManager.getDifficultySettings().EnemyHealthMultiplier);
        _scale = GameManager.SettingsManager.Resolution.Width / 1920f;
        _originalScreenWidth = GameManager.SettingsManager.Resolution.Width;

        addComponent(new Dialogable(gameManager, createDialogueScreen, Radius));
        _priceFactor = areaFactor;
        
        if (name == "Doctor")
            _priceFactor = 1f;
        if (name == "ShopChem" || name == "ShopNormal")
        {
            _name = "Shop";
            
            addComponent(new Collider(gameManager,
                () => Position,
                new Vector2(-(float)DynamicHitBoxWidth / 2, + 48)));         
        }
        else if (name == "PoliceStation")
        {
            addComponent(new Collider(gameManager,
                () => Position,
                new Vector2(-(float)DynamicHitBoxWidth / 2, + 64)));
        }
        else
        {
            addComponent(new Animatable());
            addComponent(new Collider(gameManager,
                () => Position,
                -new Vector2(DynamicHitBoxWidth / 2f, DynamicHitBoxHeight / 2f)));
        }
        
        getComponent<Collider>().IsNpc = true;
        var dynamicHitBox = getComponent<Collider>().DynamicHitBox;
        dynamicHitBox.Width = DynamicHitBoxWidth;
        dynamicHitBox.Height = DynamicHitBoxHeight;
        
        _textSeller = textSeller == "" ? getRandomText() : textSeller;
        
        
        setItems(items);
    }

    private string getRandomText()
    {
        var r = new Random();
        switch (r.Next(3))
        {
            case 0:
                return "I have a loooot good stuff...";
            case 1:
                return "What do you need?";
            case 2:
                return "Yo, wanna buy some stuff?";
        }
        return "You look like you could use something...";
    }

    private DialogueScreen createDialogueScreen(PlayerCharacterId playerCharacterId)
    {
        GameManager.SoundManager.playSoundEffect(Sfx.Psst);
        var dialogueScreen = playerCharacterId == PlayerCharacterId.Walt
            ? new DialogueScreen(GameManager,
                new Vector2(300 * _scale,
                    (GameManager.SettingsManager.Resolution.Height + 550 * _scale) * 0.2f / _scale),
                [playerCharacterId],this, null, _priceFactor)
            : new DialogueScreen(GameManager,
                new Vector2(GameManager.SettingsManager.Resolution.Width - 300 * _scale,
                    (GameManager.SettingsManager.Resolution.Height + 550 * _scale) * 0.2f / _scale),
                [playerCharacterId],this, null, _priceFactor);
        dialogueScreen.ButtonCollection.addTextElement("Sell", _textSeller, 600, 0.6f);

        foreach (var item in Stock)
        {
            dialogueScreen.ButtonCollection.addButton(item.Key + "_buy", "", 0.6f); //Text is updated in DialogueScreen
        }
        
        dialogueScreen.ButtonCollection.isDialog(true);
        if (_name is not ("Shop" or "PoliceStation" or "Doctor"))
            getComponent<Animatable>().startAnimation("approval");

        return dialogueScreen;
    }

    private void setItems(Dictionary<ItemName, int> items = null)
    {
        if (items == null)
        {
            Stock = [];
            Stock.Add(ItemName.Methylamine, 3);
            Stock.Add(ItemName.Phenylacetone, 3);
        }
        else
        {
            Dictionary<ItemName, int> newItems = [];
            foreach (var item in items)
            {
                if (item.Key != ItemName.MethSmall && item.Key != ItemName.Meth &&
                    item.Key != ItemName.Cocaine && item.Key != ItemName.Crack)
                {
                    newItems[item.Key] = item.Value;
                }
                else
                {
                    throw new Exception($"{item.Key} can not be set as Seller item!");
                }
            }
            Stock = newItems;
        }
    }

    public override void update(GameTime gameTime)
    {
        if (CanBeTalkedToWalt && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsHidden)
        {
            getComponent<Dialogable>().update(getComponent<Collider>());
        }

        if (CanBeTalkedToJesse && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsHidden)
        {
            getComponent<Dialogable>().update(getComponent<Collider>(), PlayerCharacterId.Jesse);
        }
        if (_name != "Shop" && _name != "PoliceStation")
        {
            getComponent<Animatable>().CurrentAnimation.update(gameTime);
            if (getComponent<Dialogable>().IsClosed)
                getComponent<Animatable>().startAnimation("idle");
        }
        if (_originalScreenWidth != GameManager.SettingsManager.Resolution.Width)
        {
            _scale = GameManager.SettingsManager.Resolution.Width / 1920f;
            _originalScreenWidth = GameManager.SettingsManager.Resolution.Width;
        }
        if (_cooldown <= 0)
            restock();
        else
            _cooldown -= gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void startCooldown()
    {
        _cooldown = _restockTimer;
    }
    private void restock()
    {
        foreach (var item in Stock.Keys.ToList())
        {
            Stock[item] = item switch
            {
                ItemName.Methylamine => 3,
                ItemName.Phenylacetone => 40,
                ItemName.Battery => 30,
                ItemName.CocainePlant => 2,
                ItemName.Acetone => 30,
                ItemName.ColdMedicine => 100,
                ItemName.Ammonia => 3,
                ItemName.ErlenmeyerFlask => 10,
                ItemName.Petrol => 50,
                ItemName.Hp => 15,
                ItemName.AluminumFoil => 200,
                ItemName.Matches => 130,
                ItemName.Ammo => 50,
                _ => Stock[item]
            };
        }
    }
    public override void render(GameTime gameTime)
    {
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
        {
            getComponent<Collider>().drawHitBox();
            getComponent<Collider>().drawNeighboursInRadius(Radius);
        }
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawTileCollisionRects, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawTileCollisionHitBox();

        float layerDepth = Position.Y / 100000;

        if (_name == "Shop" || _name == "PoliceStation")
        {
            var texture = GameManager.AssetManager.Images[_name];
            GameManager.SpriteBatch.Draw(texture,
                new Rectangle(new Point((int)(Position.X - texture.Width/2f), (int)(Position.Y - texture.Height/2f)), new Point(texture.Width, texture.Height)),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);
        }
        else
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
            
            GameManager.SpriteBatch.Draw(currentAnimation.SpriteSheet,
                destinationRectangle,
                sourceRectangle,
                Color.White,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);
        }
    }

    public override void loadContent()
    {
        if (_name != "Shop" && _name != "PoliceStation")
            loadAnimations();
    }

    private void loadAnimations()
    {
        const int rectangleWidth = 128; 
        const int rectangleHeight = 128;
        var animatable = getComponent<Animatable>();
        
        animatable.addAnimation("idle", new Animation(GameManager.AssetManager.Images[_name + "_Idle"], 6, rectangleWidth, rectangleHeight, 150, 0, true));
        if(_name == "Trader1" || _name == "Trader2")
            animatable.addAnimation("approval", new Animation(GameManager.AssetManager.Images[_name + "_Approval"], 8, rectangleWidth, rectangleHeight, 70, 0, false, true));
        
        animatable.CurrentAnimationName = "idle";
        animatable.CurrentAnimation.start();
    }
    
    public override void draw(GameTime gameTime) {}

    public override void saveState(GameState gameState)
    {
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.NpcSeller, Position.X, Position.Y, Stock, (int)_cooldown, _priceFactor,_name, _textSeller));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
        Position = new Vector2(gameObject.X, gameObject.Y);
        Stock = gameObject.Stock;
        _name = gameObject.Name;
        _textSeller = gameObject.Text;
        _priceFactor = gameObject.PriceFactor;
        _cooldown = gameObject.ReStockCoolDown;
    }
}
