using System.Text.Json.Serialization;
using breaking_worse.Input;
using breaking_worse.Input.Enums;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.input.KeyTypes;

public class MouseButton : IKey
{
    /// <summary>
    /// implementation of IKey for mouse buttons
    /// </summary>
    
    [JsonPropertyName("button")]
    public MouseButtons Button { get; }
    
    [JsonConstructor]
    public MouseButton(MouseButtons button)
    {
        Button = button;
    }
    
    public bool isPressed()
    {
        return Button switch
        {
            MouseButtons.LeftButton => Mouse.GetState().LeftButton == ButtonState.Pressed,
            MouseButtons.MiddleButton => Mouse.GetState().MiddleButton == ButtonState.Pressed,
            MouseButtons.RightButton => Mouse.GetState().RightButton == ButtonState.Pressed,
            MouseButtons.XButton1 => Mouse.GetState().XButton1 == ButtonState.Pressed,
            MouseButtons.XButton2 => Mouse.GetState().XButton2 == ButtonState.Pressed,
            _ => false
        };
    }
    
    public override string ToString()
    {
        return Button.ToString();
    }
}
