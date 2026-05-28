using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.Clickables;

public class TextElement : AMenuElement
{
    /// <summary>
    /// Represents a text element for the button collection.
    /// </summary>
    
    private Rectangle _hitBox;
    private readonly Texture2D _texture;
    private readonly TextBox _textBox;
    private Color _shade;
    
    private bool _isSelectedByMouse;
        
    public TextElement(GameManager gameManager, ButtonCollection buttonCollection, string text, Rectangle hitBox, float size = 0.7f)
        : base(gameManager, buttonCollection)
    {
        _texture = gameManager.AssetManager.Images["buttonBackground"];
        _hitBox = hitBox;

        var font = GameManager.AssetManager.Fonts["numbers"];
        _textBox = new TextBox(GameManager, hitBox.Center.ToVector2(), text, font, size, Color.Black);
    }

    private void checkHovering()
    {
       _isSelectedByMouse = _hitBox.Contains(GameManager.InputHandler.MousePosition);
    }
    
    public override void updateShade()
    {
        checkHovering();
        if (_isSelectedByMouse) _shade = Color.CornflowerBlue;
        else if (IsSelectedByButtonCollection) _shade = Color.Bisque;
        else if (IsSelectedBySettings) _shade = Color.LightGreen;
        else _shade = Color.White;
    }
    
    public override bool isClicked()
    {
        return false;
    }

    public override string Text { get; set; }

    public override void draw()
    {
        GameManager.SpriteBatch.Draw(_texture, _hitBox, _shade);
        _textBox.draw();
    }
}
