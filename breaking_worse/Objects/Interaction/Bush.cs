using System;
using System.IO;
using System.Linq;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Collisions;
using breaking_worse.Sound;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Interaction;

public class Bush : AGameObject
{
    private Texture2D _texture;
    private Vector2 _position;
    private const float ImageScalingFactor = 0.3f;
    private int _radius = 100;

    public Bush(GameManager gameManager, Vector2 position) : base(gameManager)
    {
        Type = NpcType.Bush;
        _position = position;
        
        addComponent(new Collider(gameManager, () => _position, Vector2.Zero));
    }

    public override void loadContent()
    {
        _texture = GameManager.AssetManager.Images["Bush"];
        var dynamicHitBox = getComponent<Collider>().DynamicHitBox;
        dynamicHitBox.Width = (int)Math.Round(_texture.Width * ImageScalingFactor * 0.7f);
        dynamicHitBox.Height = (int)Math.Round(_texture.Height * ImageScalingFactor);
    }

    public override void draw(GameTime gameTime)
    {
    }

    public override void render(GameTime gameTime)
    {
        // scales the size of the rendered image by ImageScalingFactor
        var scaledSize = new Point((int)Math.Round(_texture.Width * ImageScalingFactor), (int)Math.Round(_texture.Height * ImageScalingFactor));

        var layerDepth = (_position.Y + _texture.Height * ImageScalingFactor - 10) / 100000f;
        GameManager.SpriteBatch.Draw(_texture, new Rectangle(_position.ToPoint() - new Point((int)(_texture.Width * ImageScalingFactor * 0.15), 10), scaledSize),
            null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawHitBox();
    }
    
    public override void update(GameTime gameTime)
    {
        bool jesseInRadius = Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position, _position) < _radius * _radius;
        bool jesseInteracting = GameManager.UserActionHandler.isUserAction(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.PlayerCharacterId, UserAction.Interact, PressType.PressAndRelease);
        
        bool waltInRadius = Vector2.DistanceSquared(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position, _position) < _radius * _radius;
        bool waltInteracting = GameManager.UserActionHandler.isUserAction(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.PlayerCharacterId, UserAction.Interact, PressType.PressAndRelease);
        
        if (jesseInRadius && jesseInteracting && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInteracting)
        {
            if (GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsHidden)
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsHidden = false;
            else if (!checkForPoliceInRadius())
            {
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsHidden = true;
                GameManager.ProgressManager.updateStatistics(Statistics.BushesHidden, 1, 1);
                GameManager.SoundManager.playSoundEffect(Sfx.Bush);
            }
        }

        if (waltInRadius && waltInteracting  && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInteracting)
        {
            if (GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsHidden)
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsHidden = false;
            else if (!checkForPoliceInRadius())
            {
                GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsHidden = true;
                GameManager.ProgressManager.updateStatistics(Statistics.BushesHidden, 1, 1);
                GameManager.SoundManager.playSoundEffect(Sfx.Bush);
            }
        }
        GameManager.ProgressManager.updateAchievements(Statistics.BushesHidden, Achievement.HideXBushes);
    }

    private bool checkForPoliceInRadius()
    {
        var radius = GameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel switch
        {
            1 => 20,
            2 => 30,
            3 => 40,
            4 => 50,
            5 => 60,
            _ => 0
        };
        var neighbors = GameManager.ScreenManager.InGameScreen.CollisionManager.getNeighborsInRadius(getComponent<Collider>(), radius);
        return neighbors.Any(neighbor => neighbor.IsPolice);
    }
    public override void saveState(GameState gameState)
    {
        var name = Path.GetFileName(_texture.Name);
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.Bush, _position.X, _position.Y, name));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
        _position = new Vector2(gameObject.X, gameObject.Y);
        _texture = GameManager.AssetManager.Images[gameObject.Texture];
    }
}
