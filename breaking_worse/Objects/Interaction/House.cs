using System.IO;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Interaction;

public class House : AGameObject
{
    private Texture2D _texture;
    private float _scale;
    private Rectangle _hitBox;
    private readonly Vector2 _position;

    public House(GameManager gameManager, Texture2D texture, Vector2 position, float scale) : base(gameManager)
    {
        Type = NpcType.House;
        _texture = texture;
        _scale = scale;
        _position = position;
        _hitBox = new Rectangle((int)_position.X, (int)_position.Y, (int)(_texture.Width * _scale), (int)(_texture.Height * _scale));
    }
    
    public override void render(GameTime gameTime)
    {
        if (!_hitBox.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position) && !_hitBox.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position))
        {
            var layerDepth = (_position.Y+230) / 100000;
            GameManager.SpriteBatch.Draw(_texture, _position, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, layerDepth);
        }    
    }

    public override void update(GameTime gameTime) {}
    public override void loadContent() {}
    public override void draw(GameTime gameTime) {}

    public override void saveState(GameState gameState)
    {
        var name = Path.GetFileName(_texture.Name);
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.House, _position.X, _position.Y, name, _scale));
    }

    public override void loadState(SavedGameObject gameObject) {}
}
