using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.Clickables;

public class AchievementClickable : AMenuElement
{
    /// <summary>
    /// Represents a text element for the button collection.
    /// </summary>
    /// 
    
    private readonly Texture2D _texture;
    private readonly TextBox _textBox;
    private Color _shade;
    
    private bool _isSelectedByMouse;
        
    public AchievementClickable(GameManager gameManager, ButtonCollection buttonCollection, string text, Rectangle hitBox)
        : base(gameManager, buttonCollection)
    {
        _texture = gameManager.AssetManager.Images["buttonBackground"];
        HitBox = hitBox;

        var font = GameManager.AssetManager.Fonts["numbers"];
        _textBox = new TextBox(GameManager, HitBox.Center.ToVector2(), text, font, 0.5f, Color.Black, true);
    }

    private void checkHovering()
    {
        _isSelectedByMouse = HitBox.Contains(GameManager.InputHandler.MousePosition);
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
        GameManager.SpriteBatch.Draw(_texture, HitBox, _shade);
        if (_isSelectedByMouse || IsSelectedByButtonCollection || IsUnlocked)
            _textBox.draw();
    }
}
