using System.Collections.Generic;
using System.Text.Json.Serialization;
using breaking_worse.Input;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Player;
using breaking_worse.State.Enums;

namespace breaking_worse.State.Serializable;

public class SettingsState
{
    [JsonPropertyName("musicVolume")]
    public float MusicVolume { get; set; }
    
    [JsonPropertyName("soundEffectVolume")]
    public float SoundEffectVolume { get; set; }
    
    [JsonPropertyName("difficulty")]
    public Difficulty Difficulty { get; set; }

    [JsonPropertyName("resolution")]
    public Resolution Resolution { get; set; }
    
    [JsonPropertyName("controls")]
    public Dictionary<(InputDevice, UserAction), IKey> Controls { get; set; } = new();
}

public struct Resolution
{
    [JsonPropertyName("width")] 
    public int Width { get; set; } = 1280;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 720;
    
    [JsonPropertyName("isFullScreen")]
    public bool IsFullScreen { get; set; } = false;
    
    public Resolution(int width, int height, bool isFullScreen)
    {
        Width = width;
        Height = height;
        IsFullScreen = isFullScreen;
    }
}
