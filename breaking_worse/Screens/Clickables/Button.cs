using System.Runtime.InteropServices;
using breaking_worse.Input.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.Clickables;

public class Button : AMenuElement
{
    /// <summary>
    /// Represents a clickable button
    /// It handles hover effects and click detection, allowing to implement responsive elements
    /// </summary>
    
    private readonly Texture2D _texture;
    private TextBox _textBox;
    public Color Shade { get; set; }
    private string _text;
    private float _textSize;
    private SpriteFont _font;
    
    private bool _isSelectedByMouse;
    
    public Button(GameManager gameManager, ButtonCollection buttonCollection, string text, Rectangle hitBox, float textSize = 0.7f, string texture = null, float textOffSet = 0)
        : base(gameManager, buttonCollection)
    {
        _texture = texture == null ? gameManager.AssetManager.Images["buttonBackground"] : gameManager.AssetManager.Images[texture];
        HitBox = hitBox;
        
        _font = GameManager.AssetManager.Fonts["numbers"];
        _text = text;
        _textSize = textSize;
        Shade = Color.White;
        _textBox = new TextBox(GameManager, hitBox.Center.ToVector2() + new Vector2(0, textOffSet), text, _font, _textSize, Color.Black);
    }

    public void checkHovering()
    {
        _isSelectedByMouse = HitBox.Contains(GameManager.InputHandler.MousePosition);
    }
    
    public override void updateShade()
    {
        checkHovering();
        if (_isSelectedByMouse) Shade = Color.CornflowerBlue;
        else if (IsSelectedByButtonCollection) Shade = Color.Bisque;
        else if (IsSelectedBySettings) Shade = Color.LightGreen;
        else if (!HasUnsavedChanges && _text == "save") Shade = Color.Gray;
        else Shade = Color.White;
    }
    
    public override bool isClicked()
    {
        return _isSelectedByMouse && GameManager.UserActionHandler.isLeftMouseButtonClicked() || 
               IsSelectedByButtonCollection && ButtonCollection.isUserActionForCorrespondingPlayers(UserAction.Execute);
    }
    
    public override void draw()
    {
        GameManager.SpriteBatch.Draw(_texture, HitBox, Shade);
        _textBox.draw();
    }

    public override string Text
    {
        get => _text;
        set
        {
            _text = value;
            _textBox = new TextBox(GameManager, HitBox.Center.ToVector2(), _text, _font, _textSize, Color.Black);
        }
    }
}
