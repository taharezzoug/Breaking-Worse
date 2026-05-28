using breaking_worse.Input.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.Clickables;

public class ButtonArrow : AMenuElement
{
    /// <summary>
    /// Represents a clickable button
    /// It handles hover effects and click detection, allowing to implement responsive elements
    /// </summary>
    
    private readonly Texture2D _texture;
    private TextBox _textBox;
    private Color _shade;
    private float _rotation;
    private string _text;
    private SpriteFont _font;
    
    private bool _isSelectedByMouse;
        
    public ButtonArrow(GameManager gameManager, ButtonCollection buttonCollection, string text, Rectangle hitBox, int rotation)
        : base(gameManager, buttonCollection)
    {
        _texture = gameManager.AssetManager.Images["buttonArrowBackground"];
        HitBox = hitBox;
        _rotation = MathHelper.ToRadians(rotation);
        _text = text;
        
        _font = GameManager.AssetManager.Fonts["numbers"];
        _textBox = new TextBox(GameManager, new Vector2(HitBox.X, HitBox.Y), _text, _font, 0.6f, Color.Black);
    }

    private void checkHovering()
{
    // Calculate the rotated hitbox
    var origin = new Vector2(HitBox.Width / 2f, HitBox.Height / 2f);
    var transformedMousePosition = Vector2.Transform(GameManager.InputHandler.MousePosition.ToVector2() - HitBox.Location.ToVector2(), Matrix.CreateRotationZ(-_rotation)) + origin;

    _isSelectedByMouse = new Rectangle(Point.Zero, HitBox.Size).Contains(transformedMousePosition);
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
        return _isSelectedByMouse && GameManager.UserActionHandler.isLeftMouseButtonClicked() || 
               IsSelectedByButtonCollection && ButtonCollection.isUserActionForCorrespondingPlayers(UserAction.Execute);
    }
    
    public override void draw()
    {
        GameManager.SpriteBatch.Draw(_texture, HitBox, null, _shade, _rotation, new Vector2(_texture.Width / 2f, _texture.Height / 2f),  SpriteEffects.None, 0f);
        _textBox.draw();
    }
    
    public override string Text
    {
        get => _text;
        set
        {
            _text = value;
            _textBox = new TextBox(GameManager, new Vector2(HitBox.X, HitBox.Y), _text, _font, 0.6f, Color.Black);
        }
    }
}
