using System;
using breaking_worse.Screens.Clickables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Hud;

public class IconTextHudElement(GameManager gameManager, Vector2 position, Func<string> text, string icon, float iconScale, Vector2 textOffSet, bool isCartelIcon = false)
    : HudElement(gameManager, position)
{
    private readonly Texture2D _icon = gameManager.AssetManager.Images[icon];
    //private readonly Vector2 _textOffSet = new(75, -5);
    private int _originalWidth = gameManager.SettingsManager.Resolution.Width;
    private TextBox _textBox = new TextBox(gameManager, position + textOffSet, text(), gameManager.AssetManager.Fonts["numbers"], 1f, Color.White, false);
    private Rectangle _cartelArea = new Rectangle(new Point(1*32, 36*32), new Point(77 * 32, 107 * 32));
    
    
    public override void draw()
    {
        if (isCartelIcon && !inCartelArea()) return;
        GameManager.SpriteBatch.Draw(_icon, new Rectangle(Position.ToPoint(), new Point((int)(_icon.Width * iconScale), (int)(_icon.Height * iconScale))), Color.White);
        _textBox.draw();
    }

    public override void update()
    {
        _textBox.Text = text();
        _textBox.Position = Position + textOffSet;
    }
    
    public Vector2 newPosition
    {
        get => Position;
        set => Position = value;
    }

    private bool inCartelArea()
    {
        bool inCartelArea =
            _cartelArea.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position) ||
            _cartelArea.Contains(GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position);
        return inCartelArea;
    }
}
