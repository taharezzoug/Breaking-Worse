using System.Text.Json.Serialization;
using breaking_worse.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.input.KeyTypes;

public class GamePadButton : IKey
{
    /// <summary>
    /// implementation of IKey for gamepad buttons
    /// </summary>
    
    [JsonPropertyName("button")] 
    public Buttons Button { get; }
    
    [JsonPropertyName("playerIndex")]
    public PlayerIndex PlayerIndex { get; }
    
    [JsonConstructor]
    public GamePadButton(Buttons button, PlayerIndex playerIndex)
    {
        Button = button;
        PlayerIndex = playerIndex;
    }
    
    public bool isPressed()
    {
        return GamePad.GetState(PlayerIndex).IsButtonDown(Button);
    }
    
    public override string ToString()
    {
        return Button.ToString();
    }
}
