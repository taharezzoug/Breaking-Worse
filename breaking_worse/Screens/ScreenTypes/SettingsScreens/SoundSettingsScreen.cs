using System;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using breaking_worse.State;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.SettingsScreens;

public class SoundSettingsScreen : AMenuScreen
{
    private Slider _sfxVolumeSlider;
    private Slider _musicVolumeSlider;
    private float _previousSfxVolume;
    private float _previousMusicVolume;
    
    public SoundSettingsScreen(GameManager gameManager) : base(gameManager)
    {
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 300), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Sound Settings", Color.White);
        
        var sfxVolume = GameManager.SoundManager.SoundEffectVolume;
        var musicVolume = GameManager.SoundManager.MusicVolume;
        
        _sfxVolumeSlider = ButtonCollection.addSlider("effects", sfxVolume, "Sound Effect Volume");
        _musicVolumeSlider = ButtonCollection.addSlider("music", musicVolume, "Music Volume");
        
        _previousSfxVolume = sfxVolume;
        _previousMusicVolume = musicVolume;
        
        ButtonCollection.addButton("save", "save");
        ButtonCollection.addButton("reset", "reset");
        ButtonCollection.addButton("return", "return");
    }

    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        
        GameManager.SoundManager.SoundEffectVolume = _sfxVolumeSlider.Value;
        GameManager.SoundManager.MusicVolume = _musicVolumeSlider.Value;

        var currentSfxVolume = _sfxVolumeSlider.Value;
        var currentMusicVolume = _musicVolumeSlider.Value;
        
        if (Math.Abs(currentSfxVolume - _previousSfxVolume) > 1 || Math.Abs(currentMusicVolume - _previousMusicVolume) > 1)
        {
            ButtonCollection["save"].HasUnsavedChanges = true;
            _previousSfxVolume = currentSfxVolume;
            _previousMusicVolume = currentMusicVolume;
        }
        
        if (ButtonCollection["save"].isClicked())
        {
            GameManager.SaveManager.saveSettingsToFile(GameManager.SettingsManager.buildSettingsState());
            ButtonCollection["save"].HasUnsavedChanges = false;
        }

        if (ButtonCollection["reset"].isClicked())
        {
            _sfxVolumeSlider.Value = 50;
            _musicVolumeSlider.Value = 50;
            GameManager.SettingsManager.loadSettings(GameManager.SettingsManager.createDefaultSoundSettings());
            ButtonCollection["save"].HasUnsavedChanges = false;
            changeResolution();
        }
        
        if (ButtonCollection["return"].isClicked())
            GameManager.ScreenManager.removeFromStack(this);
    }
    
    public override void changeResolution()
    {
        yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 300), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements(); 
    }
}
