using System.Text.Json.Serialization;
using breaking_worse.Input;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.input.KeyTypes;

public class KeyboardKey : IKey
{
    /// <summary>
    /// implementation of IKey for keyboard buttons
    /// </summary>
    
    [JsonPropertyName("key")]
    public Keys Key { get; }
    
    [JsonConstructor]
    public KeyboardKey(Keys key)
    {
        Key = key;
    }
    
    public bool isPressed()
    {
        return Keyboard.GetState().IsKeyDown(Key);
    }
    
    public override string ToString()
    {
        return Key.ToString();
    }
}
