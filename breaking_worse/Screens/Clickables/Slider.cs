using System;
using breaking_worse.Input.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.Clickables;

public class Slider : AMenuElement
{
    private readonly Texture2D _trackTexture;
    private readonly Texture2D _thumbTexture;
    private readonly string _name;

    private bool _isSelectedByMouse;
    private Rectangle _thumbRectangle;
    private Color _shade;

    public Slider(GameManager gameManager, ButtonCollection buttonCollection, Rectangle hitBox, float initialValue, string name = "Slider")
        : base(gameManager, buttonCollection)
    {
        Value = initialValue;
        _name = name;
        
        _trackTexture = GameManager.AssetManager.Images["buttonBackground"];
        _thumbTexture = GameManager.AssetManager.Images["Methylamine"];
        
        HitBox = hitBox;
        _thumbRectangle = getThumbRectangle();
    }
    
    private Rectangle getThumbRectangle()
    {
        return new Rectangle((int)(HitBox.X + (Value/100) * HitBox.Width - _thumbTexture.Width),  HitBox.Y, HitBox.Height, HitBox.Height);
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
        update();
    }

    public override bool isClicked()
    {
        return _isSelectedByMouse && GameManager.UserActionHandler.isLeftMouseButtonClicked();
    }

    private bool isPressed()
    {
        return _isSelectedByMouse && GameManager.UserActionHandler.isLeftMouseButtonPressed();
    }

    private void update()
    {
        if (!isPressed()) return;
        Value = (GameManager.InputHandler.MousePosition.X - HitBox.X) / (float)HitBox.Width * 100;
        _thumbRectangle.X = GameManager.InputHandler.MousePosition.X - _thumbTexture.Width;
    }

    public float Value { get; set; }

    public override string Text { get; set; }

    public override void draw()
    {
        // Draw the track
        GameManager.SpriteBatch.Draw(_trackTexture, HitBox, _shade);

        // Draw the thumb
        GameManager.SpriteBatch.Draw(_thumbTexture, _thumbRectangle, Color.White);
        
        var font = GameManager.AssetManager.Fonts["gabriele"];
        var valueText = $"{_name} Value: {Math.Round(Value)}";
        var textPosition = new Vector2(HitBox.X, HitBox.Y - 25); 
        GameManager.SpriteBatch.DrawString(font, valueText, textPosition, Color.Red, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
    }
    
    public void updateWithKeys()
    {
        if (ButtonCollection.isUserActionForCorrespondingPlayers(UserAction.Left))
        {
            Value = MathHelper.Clamp(Value - 10, 0, 100); 
            _thumbRectangle = getThumbRectangle(); 
        }
        else if (ButtonCollection.isUserActionForCorrespondingPlayers(UserAction.Right))
        {
            Value = MathHelper.Clamp(Value + 10, 0, 100);
            _thumbRectangle = getThumbRectangle();
        }
    }
}