using System.Collections.Generic;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Collisions;
using breaking_worse.Sound;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Interaction;

public class Door : AGameObject
{
    private const int FrameWidth = 116;
    private const int FrameHeight = 116; 
    private const int AnimationSpeed = 60; 
    private const int TotalFrames = 5; 
    private readonly Texture2D _doorTexture;

    private int _index;
    private float _timer;
    private Rectangle _currentFrameRectangle;
    private readonly int _radius = 100;
    private Vector2 _position = new ((147 * 32) - 70, (76 * 32) - 70);
    private readonly Vector2 _logicalPosition = new ((147 * 32), (76 * 32) + 64);
    private readonly List<int> _funnyNumbers = [0, 110, 220, 340, 450];
    // HitBox dimensions
    private const int DynamicHitBoxWidth = 40;
    private const int DynamicHitBoxHeight = 40;

    private bool _isAnimating;
    public bool IsOpen { get; private set; }

    public Door(GameManager gameManager) : base(gameManager)
    {
        Type = NpcType.Door;
        var colPosition = _position;
        colPosition.X += 35;
        colPosition.Y += 80;
        _doorTexture = GameManager.AssetManager.Images["door"];
        _currentFrameRectangle = new Rectangle(0, 0, FrameWidth, FrameHeight);
        addComponent(new Collider(gameManager, () => colPosition,
           Vector2.Zero));
        var dynamicHitBox = getComponent<Collider>().DynamicHitBox;
        dynamicHitBox.Width = DynamicHitBoxWidth;
        dynamicHitBox.Height = DynamicHitBoxHeight;
    }

    public override void update(GameTime gameTime)
    {
        getComponent<Collider>().ExcludeFromCollisions = IsOpen;
        if (GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt != null && GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse != null)
        {
            bool inRadius = Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position, _logicalPosition) < _radius * _radius &&
                            Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position, _logicalPosition) < _radius * _radius;
            
            bool oneInRadius = Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position, _logicalPosition) < _radius * _radius ||
                               Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position, _logicalPosition) < _radius * _radius;
            
            bool waltInteracting = GameManager.UserActionHandler.isUserAction(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.PlayerCharacterId, UserAction.Interact, PressType.PressAndRelease);
            bool jesseInteracting = GameManager.UserActionHandler.isUserAction(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.PlayerCharacterId, UserAction.Interact, PressType.PressAndRelease);


            if (inRadius && (waltInteracting || jesseInteracting))
            {
                if (!IsOpen)
                {
                    openDoor();
                    GameManager.SoundManager.playSoundEffect(Sfx.DoorOpen);
                }
            }
            else if (!oneInRadius)
            {
                if (IsOpen)
                {
                    closeDoor();
                    GameManager.SoundManager.playSoundEffect(Sfx.DoorClose);
                }
            }

            if (_isAnimating)
            {
                _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_timer >= AnimationSpeed)
                {
                    _timer = 0f; 
                    _index += IsOpen ? 1 : -1;
                    
                    if (IsOpen && _index >= TotalFrames)
                    {
                        _index = TotalFrames - 1;
                        _isAnimating = false;
                    }
                    else if (!IsOpen && _index < 0)
                    {
                        _index = 0;
                        _isAnimating = false;
                    }
                    
                    _currentFrameRectangle.Y = _funnyNumbers[_index];
                }
            }
        }
    }

    private void openDoor()
    {
        IsOpen = true;
        _isAnimating = true;
        _index = 0; 
    }

    private void closeDoor()
    {
        IsOpen = false;
        _isAnimating = true;
        _index = TotalFrames - 1;
    }

    public override void render(GameTime gameTime)
    {
        float layerDepth = (_logicalPosition.Y - 32) / 100000f;
        GameManager.SpriteBatch.Draw(_doorTexture, new Rectangle(_position.ToPoint(), _currentFrameRectangle.Size), _currentFrameRectangle, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
        {
            getComponent<Collider>().drawHitBox();
        }
    }

    public override void loadContent()
    {
    }

    public override void draw(GameTime gameTime)
    {
    }

    public override void saveState(GameState gameState)
    {
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.Door, _position.X, _position.Y, IsOpen));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
    }
}
