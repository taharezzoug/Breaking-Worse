using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Input;
using breaking_worse.Input.Enums;
using breaking_worse.input.KeyTypes;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.State;

public class SettingsManager
{
    private readonly GameManager _gameManager;
    
    // bindable controls
    public Dictionary<(InputDevice, UserAction), IKey> Controls { get; private set; }
    
    // unbindable controls
    public readonly IKey LeftMouseButton = new MouseButton(MouseButtons.LeftButton);
    public readonly IKey Enter = new KeyboardKey(Keys.Enter);
    public readonly IKey Tab = new KeyboardKey(Keys.Tab);
    public readonly IKey DrawHitBoxKey = new KeyboardKey(Keys.D9);
    public readonly IKey DrawTileCollisionRects = new KeyboardKey(Keys.D8);
    
    public Resolution Resolution { get; set; }

    public void changeResolution(int width, int height)
    {
        _gameManager.GraphicsDeviceManager.PreferredBackBufferWidth = width;
        _gameManager.GraphicsDeviceManager.PreferredBackBufferHeight = height;
        _gameManager.GraphicsDeviceManager.ApplyChanges();
        
        Resolution = new Resolution(width, height, Resolution.IsFullScreen);

        _gameManager.ScreenManager.changeScreenResolution();
    }
    
    public void isFullScreen(bool isFullScreen)
    {
        _gameManager.GraphicsDeviceManager.IsFullScreen = isFullScreen;
        _gameManager.GraphicsDeviceManager.ApplyChanges();
        
        Resolution = new Resolution(Resolution.Width, Resolution.Height, isFullScreen);
        _gameManager.ScreenManager.changeScreenResolution();
    }
    
    public float MusicVolume
    {
        get => _gameManager.SoundManager.MusicVolume;
        set => _gameManager.SoundManager.MusicVolume = value;
    }
    
    public float SfxVolume
    {
        get => _gameManager.SoundManager.SoundEffectVolume;
        set => _gameManager.SoundManager.SoundEffectVolume = value;
    }
    
    public Difficulty Difficulty
    {
        get;
        set;
    } = Difficulty.Normal;
    
    public SettingsManager(GameManager gameManager)
    {
        _gameManager = gameManager;
        loadSettings(_gameManager.SaveManager.loadSettingsFromFile());
    }
    
    public SettingsState buildSettingsState()
    {
        SettingsState settingsState = new()
        {
            MusicVolume = (int)MusicVolume,
            SoundEffectVolume = (int)SfxVolume,
            Difficulty = Difficulty,
            Resolution = Resolution,
            Controls = Controls
        };
        return settingsState;
    }

    public void loadSettings(SettingsState settingsState)
    {
        Difficulty = settingsState.Difficulty;
        SfxVolume = settingsState.SoundEffectVolume;
        MusicVolume = settingsState.MusicVolume;
        Resolution = settingsState.Resolution;
        Controls = settingsState.Controls;

        _gameManager.GraphicsDeviceManager.PreferredBackBufferWidth = Resolution.Width;
        _gameManager.GraphicsDeviceManager.PreferredBackBufferHeight = Resolution.Height;
        _gameManager.GraphicsDeviceManager.IsFullScreen = Resolution.IsFullScreen;
        _gameManager.GraphicsDeviceManager.ApplyChanges();
    }

    public SettingsState createDefaultSoundSettings()
    {
        SettingsState settingsState = new()
        {
            MusicVolume = 50,
            SoundEffectVolume = 50,
            Difficulty = Difficulty,
            Resolution = Resolution,
            Controls = Controls
        };
        return settingsState;
    }

    public SettingsState createDefaultDifficultySettings()
    {
        SettingsState settingsState = new()
        {
            MusicVolume = MusicVolume,
            SoundEffectVolume = SfxVolume,
            Difficulty = Difficulty.Normal,
            Resolution = Resolution,
            Controls = Controls
        };
        return settingsState;
    }

    public SettingsState createDefaultGraphicsSettings()
    {
        SettingsState settingsState = new()
        {
            MusicVolume = MusicVolume,
            SoundEffectVolume = SfxVolume,
            Difficulty = Difficulty,
            Resolution = new Resolution { Width = 1920, Height = 1080, IsFullScreen = false },
            Controls = Controls
        };
        return settingsState;
    }

    public SettingsState createDefaultPlayerSettings(InputDevice inputDevice)
    {
        switch (inputDevice)
        {
            case InputDevice.KeyboardLeft:
            {
                foreach (var key in Controls.Keys.ToList().Where(key => key.Item1 == inputDevice))
                {
                    Controls[key] = key.Item2 switch
                    {
                        UserAction.Down => new KeyboardKey(Keys.S),
                        UserAction.Up => new KeyboardKey(Keys.W),
                        UserAction.Left => new KeyboardKey(Keys.A),
                        UserAction.Right => new KeyboardKey(Keys.D),
                        UserAction.Execute => new KeyboardKey(Keys.Space),
                        UserAction.Interact => new KeyboardKey(Keys.F),
                        UserAction.Inventory => new KeyboardKey(Keys.Tab),
                        UserAction.SwitchWeapon => new KeyboardKey(Keys.R),
                        UserAction.PauseMenu => new KeyboardKey(Keys.Escape),
                        _ => Controls[key]
                    };
                }
                break;
            }
            case InputDevice.KeyboardRight:
            {
                foreach (var key in Controls.Keys.ToList().Where(key => key.Item1 == inputDevice))
                {
                    Controls[key] = key.Item2 switch
                    {
                        UserAction.Down => new KeyboardKey(Keys.Down),
                        UserAction.Up => new KeyboardKey(Keys.Up),
                        UserAction.Left => new KeyboardKey(Keys.Left),
                        UserAction.Right => new KeyboardKey(Keys.Right),
                        UserAction.Execute => new KeyboardKey(Keys.NumPad1),
                        UserAction.Interact => new KeyboardKey(Keys.NumPad2),
                        UserAction.Inventory => new KeyboardKey(Keys.NumPad6),
                        UserAction.SwitchWeapon => new KeyboardKey(Keys.NumPad3),
                        UserAction.PauseMenu => new KeyboardKey(Keys.NumPad9),
                        _ => Controls[key]
                    };
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(inputDevice), inputDevice, null);
        }

SettingsState settingsState = new()
        {
            MusicVolume = MusicVolume,
            SoundEffectVolume = SfxVolume,
            Difficulty = Difficulty,
            Resolution =  Resolution,
            Controls = Controls
        };
        return settingsState;
    }
    public static SettingsState createDefaultSettingsState()
    {
        SettingsState settingsState = new()
        {
            MusicVolume = 50,
            SoundEffectVolume = 50,
            Difficulty = Difficulty.Normal,
            Resolution = new Resolution {Width = 1920, Height = 1080, IsFullScreen = false},
            Controls = new Dictionary<(InputDevice, UserAction), IKey>
            { 
                // keyboard left
                { (InputDevice.KeyboardLeft, UserAction.Up), new KeyboardKey(Keys.W) },
                { (InputDevice.KeyboardLeft, UserAction.Down), new KeyboardKey(Keys.S) },
                { (InputDevice.KeyboardLeft, UserAction.Left), new KeyboardKey(Keys.A) },
                { (InputDevice.KeyboardLeft, UserAction.Right), new KeyboardKey(Keys.D) },
                { (InputDevice.KeyboardLeft, UserAction.Interact), new KeyboardKey(Keys.F) },
                { (InputDevice.KeyboardLeft, UserAction.Inventory), new KeyboardKey(Keys.Tab) },
                { (InputDevice.KeyboardLeft, UserAction.Execute), new KeyboardKey(Keys.Space) },
                { (InputDevice.KeyboardLeft, UserAction.SwitchWeapon), new KeyboardKey(Keys.R) },
                { (InputDevice.KeyboardLeft, UserAction.PauseMenu), new KeyboardKey(Keys.Escape) },
                // keyboard right
                { (InputDevice.KeyboardRight, UserAction.Up), new KeyboardKey(Keys.Up) },
                { (InputDevice.KeyboardRight, UserAction.Down), new KeyboardKey(Keys.Down) },
                { (InputDevice.KeyboardRight, UserAction.Left), new KeyboardKey(Keys.Left) },
                { (InputDevice.KeyboardRight, UserAction.Right), new KeyboardKey(Keys.Right) },
                { (InputDevice.KeyboardRight, UserAction.Interact), new KeyboardKey(Keys.NumPad2) },
                { (InputDevice.KeyboardRight, UserAction.Inventory), new KeyboardKey(Keys.NumPad6) },
                { (InputDevice.KeyboardRight, UserAction.Execute), new KeyboardKey(Keys.NumPad1) },
                { (InputDevice.KeyboardRight, UserAction.SwitchWeapon), new KeyboardKey(Keys.NumPad3) },
                { (InputDevice.KeyboardRight, UserAction.PauseMenu), new KeyboardKey(Keys.NumPad9) },
                // gamepad one
                { (InputDevice.GamePadOne, UserAction.Up), new GamePadButton(Buttons.DPadUp, PlayerIndex.One) },
                { (InputDevice.GamePadOne, UserAction.Down), new GamePadButton(Buttons.DPadDown, PlayerIndex.One) },
                { (InputDevice.GamePadOne, UserAction.Left), new GamePadButton(Buttons.DPadLeft, PlayerIndex.One) },
                { (InputDevice.GamePadOne, UserAction.Right), new GamePadButton(Buttons.DPadRight, PlayerIndex.One) },
                { (InputDevice.GamePadOne, UserAction.Interact), new GamePadButton(Buttons.A, PlayerIndex.One) },
                { (InputDevice.GamePadOne, UserAction.Inventory), new GamePadButton(Buttons.Back, PlayerIndex.One) },
                { (InputDevice.GamePadOne, UserAction.Execute), new GamePadButton(Buttons.X, PlayerIndex.One) },
                { (InputDevice.GamePadOne, UserAction.SwitchWeapon), new GamePadButton(Buttons.Y, PlayerIndex.One) },
                { (InputDevice.GamePadOne, UserAction.PauseMenu), new GamePadButton(Buttons.Start, PlayerIndex.One) },
                // gamepad two
                { (InputDevice.GamePadTwo, UserAction.Up), new GamePadButton(Buttons.DPadUp, PlayerIndex.Two) },
                { (InputDevice.GamePadTwo, UserAction.Down), new GamePadButton(Buttons.DPadDown, PlayerIndex.Two) },
                { (InputDevice.GamePadTwo, UserAction.Left), new GamePadButton(Buttons.DPadLeft, PlayerIndex.Two) },
                { (InputDevice.GamePadTwo, UserAction.Right), new GamePadButton(Buttons.DPadRight, PlayerIndex.Two) },
                { (InputDevice.GamePadTwo, UserAction.Interact), new GamePadButton(Buttons.A, PlayerIndex.Two) },
                { (InputDevice.GamePadTwo, UserAction.Inventory), new GamePadButton(Buttons.Back, PlayerIndex.Two) },
                { (InputDevice.GamePadTwo, UserAction.Execute), new GamePadButton(Buttons.X, PlayerIndex.Two) },
                { (InputDevice.GamePadTwo, UserAction.SwitchWeapon), new GamePadButton(Buttons.Y, PlayerIndex.Two) },
                { (InputDevice.GamePadTwo, UserAction.PauseMenu), new GamePadButton(Buttons.Start, PlayerIndex.Two) } 
            }
        };
        return settingsState;
    }
    public DifficultySettings getDifficultySettings()
    {
        return Difficulty switch
        {
            Difficulty.Easy => new DifficultySettings(playerHealthMultiplier: 1.5f, enemyHealthMultiplier: 0.8f,
                timerMultiplier: 1.2f, npcSellMultiplier: 0.7f, npcBuyMultiplier: 1.5f, penaltyMultiplier: 0.7f, wantedLevelMultiplier: 0.5f, suspiciousActivityMultiplier:0.5f),
            Difficulty.Normal => new DifficultySettings(playerHealthMultiplier: 1.0f, enemyHealthMultiplier: 1.0f,
                timerMultiplier: 1.0f, npcSellMultiplier: 1.0f, npcBuyMultiplier: 1.0f, penaltyMultiplier: 1.0f, wantedLevelMultiplier: 1.0f, suspiciousActivityMultiplier: 1.0f),
            Difficulty.Hard => new DifficultySettings(playerHealthMultiplier: 0.7f, enemyHealthMultiplier: 1.3f,
                timerMultiplier: 0.8f, npcSellMultiplier: 1.5f, npcBuyMultiplier: 0.7f, penaltyMultiplier: 1.5f, wantedLevelMultiplier: 1.5f, suspiciousActivityMultiplier: 1.5f),
            _ => throw new ArgumentOutOfRangeException(nameof(Difficulty), "Unknown difficulty")
        };
    }
}
public class DifficultySettings(
    float playerHealthMultiplier,
    float enemyHealthMultiplier,
    float timerMultiplier,
    float npcSellMultiplier,
    float npcBuyMultiplier,
    float penaltyMultiplier,
    float wantedLevelMultiplier,
    float suspiciousActivityMultiplier)
{
    public float PlayerHealthMultiplier { get; set; } = playerHealthMultiplier;
    public float EnemyHealthMultiplier { get; set; } = enemyHealthMultiplier;
    public float TimerMultiplier { get; set; } = timerMultiplier;
    public float NpcSellMultiplier { get; set; } = npcSellMultiplier;
    public float NpcBuyMultiplier { get; set; } = npcBuyMultiplier;
    public float PenaltyMultiplier { get; set; } = penaltyMultiplier;
    public float WantedLevelMultiplier { get; set; } = wantedLevelMultiplier;
    public float SuspiciousActivityMultiplier { get; set; } = suspiciousActivityMultiplier;
}
