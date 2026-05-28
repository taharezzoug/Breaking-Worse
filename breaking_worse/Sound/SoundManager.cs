using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace breaking_worse.Sound;

public class SoundManager(GameManager gameManager)
{
    /// <summary>
    /// Loads the music and sound effects tracks. Manages their behaviour through method calls
    /// </summary>

    private const float PercentageToValue = 0.01f;
    
    private readonly Dictionary<string, Song> _musicTracks = new();
    private readonly Dictionary<string, SoundEffect> _soundEffects = new();
    private Song _currentSong;
    private readonly bool _isMusicLooping = true;
    private float _soundEffectVolume = 100f;
    private float _musicVolume = 0f;
    private bool _stepFlag;
    private bool _isStepSoundPlaying;
    private readonly List<string> _soundEffectNames = getEnumNamesAsStringList<Sfx>();
    
    private readonly int _maxConcurrentSoundEffects = 40; 
    private int _currentPlayingSoundEffects; 


    // only to be used by SettingsManager
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = MathHelper.Clamp(value, 0f, 100f); // Sync the internal field
            MediaPlayer.Volume = MathHelper.Clamp(_musicVolume * PercentageToValue, 0f, 1f);
        }
    }


    // only to be used by SettingsManager
    public float SoundEffectVolume
    {
        get => _soundEffectVolume;
        set => _soundEffectVolume = MathHelper.Clamp(value, 0f, 100f);
    }

    public void loadContent()
    {
        loadMusic(gameManager.AssetManager.Music);
        loadSoundEffects(gameManager.AssetManager.SoundEffects);
    }

    private void loadMusic(Dictionary<string, Song> musics)
    {
        foreach (var musicKey in musics.Keys)
        {
            _musicTracks[musicKey] = musics[musicKey];
        }
    }

    private void loadSoundEffects(Dictionary<string, SoundEffect> soundEffects)
    {
        foreach (var soundEffectKey in soundEffects.Keys)
        {
            _soundEffects[soundEffectKey] = soundEffects[soundEffectKey];
        }
    }

    public void playMusic(string musicName)
    {
        _currentSong = _musicTracks[musicName];
        MediaPlayer.IsRepeating = _isMusicLooping;
        MediaPlayer.Play(_currentSong);
    }

    public async void playSoundEffect(Sfx sfx)
    {

        if (_currentPlayingSoundEffects >= _maxConcurrentSoundEffects) return;
        
        var soundName = _soundEffectNames[(int)sfx];

        if (sfx == Sfx.Step)
        {
            if (_isStepSoundPlaying)
            {
                return; // Prevent overlapping step sounds
            }

            soundName = _stepFlag ? "StepSound1" : "StepSound2";
            _stepFlag = !_stepFlag;

            if (gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInCar ||
                gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInCar)
            {
                soundName = "carSound";
            }

            if (!_soundEffects.TryGetValue(soundName, out var stepSound)) return;
            
            _currentPlayingSoundEffects++;
            stepSound.Play(_soundEffectVolume * PercentageToValue * (soundName == "carSound" ? 0.1f : 1f), 0.0f, 0.0f);
            _isStepSoundPlaying = true;

            // Use a task delay to reset the flag after the sound duration
            await Task.Delay((int)(stepSound.Duration.TotalSeconds * 500));
            _isStepSoundPlaying = false;
            _currentPlayingSoundEffects--;

            return;
        }

        if (_soundEffects.TryGetValue(soundName, out var effect))
        {
            _currentPlayingSoundEffects++;
            effect.Play(_soundEffectVolume * PercentageToValue, 0.0f, 0.0f);
            await Task.Delay((int)(effect.Duration.TotalMilliseconds));
            _currentPlayingSoundEffects--;
        }
    }


    public void stopMusic()
    {
        MediaPlayer.Stop();
        _currentSong = null;
    }

    public void pauseMusic()
    {
        MediaPlayer.Pause();
    }

    public void resumeMusic()
    {
        MediaPlayer.Resume();
    }
    
    private static List<string> getEnumNamesAsStringList<T>() where T : Enum
    {
        // Get the names of the enum and convert them to a list
        return [..Enum.GetNames(typeof(T))];
    }
}
