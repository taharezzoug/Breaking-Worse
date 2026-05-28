using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.Clickables;

public class TextBox
{
    /// <summary>
    /// Creates a Textbox to write text on a screen
    /// <param name="font">font to write with</param>
    /// <param name="text">which text should be written</param>
    /// <param name="position">where to draw the text</param>
    /// <param name="scale">choose the size of the text</param>
    /// <param name="color">choose the color of the text</param>
    /// </summary>

    private readonly GameManager _gameManager;
    
    private Vector2 _position;
    
    private readonly SpriteFont _font;
    private string _text;
    private float _scale;
    private readonly Color _color;
    
    public TextBox(GameManager gameManager, Vector2 position, string text, SpriteFont font, float size, Color color, bool isCentered = true)
    {
        var scale = Math.Min(gameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f,
            gameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height / 1080f);
        _gameManager = gameManager;
        _position = position;
        _font = font;
        _text = text;
        _scale = size * scale;       
        _color = color;

        if (isCentered)
            _position = centerPosition();
    }

    // returns box around the text based on layout
    private Vector2 centerPosition()
    {
        var textSize = _font.MeasureString(_text);  // size of the written Text
        return _position - textSize * _scale / 2f;
    }

    public void draw()
    {
        _gameManager.SpriteBatch.DrawString(_font, _text, _position, _color, 0f, 
            Vector2.Zero, _scale, SpriteEffects.None, 0f);
    }

    public string Text
    {
        get => _text;
        set => _text = value;
    }
    
    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }
}
