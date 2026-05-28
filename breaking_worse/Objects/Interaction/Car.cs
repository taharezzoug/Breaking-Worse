using System;
using System.IO;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Collisions;
using breaking_worse.Sound;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Interaction;

public class Car : AGameObject
{
    private Texture2D _texture;
    private const float ImageScalingFactor = 1f;
    private readonly int _radius = 100;
    private bool _waltInCar;
    private bool _jesseInCar;
    private float DirectionFov;
    // HitBox stuff
    private const int DynamicHitBoxWidth = 84;
    private const int DynamicHitBoxHeight = 84;

    public Car(GameManager gameManager, Vector2 position) : base(gameManager)
    {
        Type = NpcType.Car;
        Position = position;
        addComponent(new Collider(gameManager, () => Position, 
            -new Vector2(DynamicHitBoxWidth / 2f, DynamicHitBoxHeight / 2f)));
        var dynamicHitBox = getComponent<Collider>().DynamicHitBox;
        dynamicHitBox.Width = DynamicHitBoxWidth;
        dynamicHitBox.Height = DynamicHitBoxHeight;
    }

    public override void loadContent()
    {
        _texture = GameManager.AssetManager.Images["jeep"];
    }

    public override void draw(GameTime gameTime)
    {
    }

    public override void render(GameTime gameTime)
    {
        float layerDepth = Position.Y / 100000f;
        
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawHitBox();
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawTileCollisionRects, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawTileCollisionHitBox();
        
        
        GameManager.SpriteBatch.Draw(_texture, Position, null, Color.White, DirectionFov, new Vector2(32, 44), ImageScalingFactor, SpriteEffects.None, layerDepth);
    }
    
    private void enterCar(bool walt)
    {
        GameManager.SoundManager.playSoundEffect(Sfx.CarStartup);
        if (walt)
        {
            _waltInCar = true;
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position = Position - new Vector2(32, 32);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>().DynamicHitBox.Width = 84;
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>().DynamicHitBox.Height = 84;
        }
        else
        {
            _jesseInCar = true;
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position = Position - new Vector2(32, 32);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>().DynamicHitBox.Width = 84;
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>().DynamicHitBox.Height = 84;
        }
        getComponent<Collider>().ExcludeFromCollisions = true;
    }
    
    public override void update(GameTime gameTime)
    {
        if (GameManager.UserActionHandler.isUserAction(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.PlayerCharacterId, UserAction.Interact, PressType.PressAndRelease) &&
            Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position, Position) < _radius * _radius)
        {
            if (_jesseInCar)
            {
                _jesseInCar = false;
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position = Position;
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>().DynamicHitBox.Width = 20;
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>().DynamicHitBox.Height = 20;
                getComponent<Collider>().ExcludeFromCollisions = false;
            }
            else if (!_waltInCar && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInteracting)
            {
                enterCar(false);
            }
        }

        if (GameManager.UserActionHandler.isUserAction(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.PlayerCharacterId, UserAction.Interact, PressType.PressAndRelease) && 
            Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position, Position) < _radius * _radius)
        {
            if (_waltInCar)
            {
                _waltInCar = false;
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position = Position;
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>().DynamicHitBox.Width = 20;
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>().DynamicHitBox.Height = 20;
                getComponent<Collider>().ExcludeFromCollisions = false;
            }
            else if (!_jesseInCar && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInteracting)
            {
                enterCar(true);
            }
        }

        
        GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInCar = _jesseInCar;
        GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInCar = _waltInCar;

        if (_waltInCar)
        {
            Position = GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position + new Vector2(32, 32);
            DirectionFov = (float)(Math.Atan2(
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.DirectionFov.Y,
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.DirectionFov.X) + Math.PI / 2f);
            _texture = GameManager.AssetManager.Images["jeepWithWalt"];
        }

        if (_jesseInCar)
        {
            Position = GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position + new Vector2(32, 32);
            DirectionFov = (float)(Math.Atan2(
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.DirectionFov.Y,
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.DirectionFov.X) + Math.PI / 2f);
            _texture = GameManager.AssetManager.Images["jeepWithJesse"];
        }
        if (!_waltInCar && !_jesseInCar) _texture = GameManager.AssetManager.Images["jeep"];
        
    }

    public override void saveState(GameState gameState)
    {
        var name = Path.GetFileName(_texture.Name);
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.Car, Position.X, Position.Y, name, _waltInCar, _jesseInCar));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
        Position = new Vector2(gameObject.X, gameObject.Y);
        _texture = GameManager.AssetManager.Images[gameObject.Texture];
        _waltInCar = gameObject.WaltInCar; 
        _jesseInCar = gameObject.JesseInCar;
        if (_waltInCar) enterCar(true);
        if (_jesseInCar) enterCar(false);
    }
}
