using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Combat;
using breaking_worse.Objects.Items;
using breaking_worse.Sound;
using breaking_worse.State;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Player;

public class PlayerState(GameManager gameManager)
{
    /// <summary>
    /// The Player State shared between the two characters.
    /// </summary>
    private readonly DifficultySettings _settings = gameManager.SettingsManager.getDifficultySettings();
    // timer
    private const int Duration = 3000;
    private double _timeCounter;
    public int Timer { get; set; } = (int) (1000 * gameManager.SettingsManager.getDifficultySettings().TimerMultiplier);
    
    private int _lastJailTime = 10000;
    private int _timeSinceLastJail;
    
    // time until players are teleported after getting caught
    public DateTime? CaughtTime { get; set; }
    private readonly TimeSpan _caughtTimeSpan = TimeSpan.FromSeconds(6);
    public bool IsCaught { get; set; }
    
    // wanted level
    private const int WantedLevelResetRadius = 8;
    private TimeSpan _resetThreshold;
    public DateTime LastTimeSeen { get; set; }
    public float RemainingWantedTime { get; private set; }
    public int SuspiciousActivityCounter { get; set; }
    public int WantedLevel { get; private set; }
    public bool WillShoot { get; set; }
    public float AggroCooldown { get; set; }= 100f;
    
    // status
    public int Health { get; set; }
    public int Arrests { get; set; }
    public int Deaths { get; set; }
    public float StepsTaken { get;  private set; }
    private bool _playOnceSirens = true;

    public double TimeSinceLastDamage { get; set; }
    
    private double _regenInterval = 10;
    
    private int _maxHealth = (int)(20 * gameManager.SettingsManager.getDifficultySettings().PlayerHealthMultiplier);

    
    // inventory
    public Inventory Inventory { get; } = new(new Dictionary<ItemName, int>(), (int)(2000 * gameManager.SettingsManager.getDifficultySettings().NpcBuyMultiplier), 25);
    
    // movement
    public float MovementSpeed => 0.2f;
    
    public bool shouldTeleport()
    {
        return IsCaught && DateTime.Now - CaughtTime >= _caughtTimeSpan;
    }

    private void decreaseTimer()
    {
        if (!isTimerOver() && Timer != int.MaxValue)
        {
            Timer--;
        }
    }

    public void update(GameTime gameTime)
    {
        WantedLevel = calculateWantedLevel();
        _resetThreshold = TimeSpan.FromSeconds(calculateWantedLevelResetThreshold());
        
        var neighborsInRadiusWalt = gameManager.ScreenManager.InGameScreen.CollisionManager.getNeighborsInRadius(gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>(), WantedLevelResetRadius);
        var neighborsInRadiusJesse = gameManager.ScreenManager.InGameScreen.CollisionManager.getNeighborsInRadius(gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>(), WantedLevelResetRadius);
        if (neighborsInRadiusWalt.Any(neighbor => neighbor.IsPolice) ||
            neighborsInRadiusJesse.Any(neighbor => neighbor.IsPolice))
            LastTimeSeen = DateTime.Now;
        else if (DateTime.Now - LastTimeSeen > _resetThreshold)
            SuspiciousActivityCounter = 0;
        
        RemainingWantedTime = (float)Math.Round(Math.Max(0, (float)(_resetThreshold.TotalSeconds - (DateTime.Now - LastTimeSeen).TotalSeconds)), 1);
        
        _timeCounter += gameTime.ElapsedGameTime.Milliseconds;
        if (_timeCounter >= Duration)
        {
            decreaseTimer();
            _timeCounter = 0;
        }
        if (Health < 0) Health = 0;
        if (Health <= 0 || IsCaught)
        {
            WillShoot = false;
            gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsHidden = false;
            gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsHidden = false;
            SuspiciousActivityCounter = 0;
        }
        _timeSinceLastJail = _lastJailTime - Timer;
        gameManager.ProgressManager.updateStatistics(Statistics.LastArrest, _timeSinceLastJail, 2);
        TimeSinceLastDamage += gameTime.ElapsedGameTime.TotalSeconds;
        if (TimeSinceLastDamage >= _regenInterval) TimeSinceLastDamage = _regenInterval;
        if (!(TimeSinceLastDamage >= _regenInterval) || Health >= _maxHealth) return;
        gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<ActiveCombatant>().Health++;
        gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<ActiveCombatant>().Health++;
        gameManager.ScreenManager.InGameScreen.PlayerState.balanceHealth();
        TimeSinceLastDamage = 0;
    }

    private bool isTimerOver()
    {
        return Timer <= 0 && Timer != int.MaxValue;
    }
    
    public void wentToJail()
    {
        _lastJailTime = Timer;
    }

    private int calculateWantedLevel()
    {
        var multiplier = 1 / _settings.WantedLevelMultiplier;
        int wantedLevel;
        if (SuspiciousActivityCounter < 5 * multiplier) wantedLevel = 0;
        else if (SuspiciousActivityCounter < 20 * multiplier) wantedLevel = 1;
        else if (SuspiciousActivityCounter < 50 * multiplier) wantedLevel = 2;
        else if (SuspiciousActivityCounter < 120 * multiplier) wantedLevel = 3;
        else if (SuspiciousActivityCounter < 250 * multiplier) wantedLevel = 4;
        else wantedLevel = 5;
        updateHighestWantedLevel(wantedLevel);
        
        if (wantedLevel == 5)
        {
            if (_playOnceSirens) gameManager.SoundManager.playSoundEffect(Sfx.Police);
            _playOnceSirens = false;
        }
        else _playOnceSirens = true;
        
        return wantedLevel;
    }

    private double calculateWantedLevelResetThreshold()
    {
        var multiplier = _settings.WantedLevelMultiplier;
        return WantedLevel switch
        {
            1 => 20 * multiplier,
            2 => 30 * multiplier,
            3 => 45 * multiplier,
            4 => 60 * multiplier,
            5 => 75 * multiplier,
            _ => 0
        };
    }
    
    private void updateHighestWantedLevel(int wantedLevel)
    {
        if (wantedLevel > gameManager.ProgressManager.getStatistics(Statistics.HighestWantedLevel))
        {
            gameManager.ProgressManager.updateStatistics(Statistics.HighestWantedLevel, wantedLevel, 2);
        }
    }
    
    public void balanceHealth()
    {
        var health = Math.Min(
            gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<ActiveCombatant>().Health,
            gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<ActiveCombatant>().Health);
        gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<ActiveCombatant>().Health = health;
        gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<ActiveCombatant>().Health = health;
        Health = health;
    }
    
    public void increaseSteps(float steps)
    {
        StepsTaken += steps;
        gameManager.ProgressManager.updateStatistics(Statistics.StepsTaken, (int) StepsTaken, 0);
        gameManager.ProgressManager.updateAchievements(Statistics.StepsTaken, Achievement.FirstSteps);
    }

    public void saveGameState(GameState gameState)
    {
        var textureName = Inventory.SelectedRecipe.Name.Substring(Inventory.SelectedRecipe.Name.LastIndexOf('/') + 1);
        gameState.SavedPlayerState = new SavedPlayerState(Timer, Health, SuspiciousActivityCounter, Arrests, Deaths, Inventory.Items, Inventory.Money, WillShoot, AggroCooldown, textureName);
    }

    public void loadGameState(SavedPlayerState savedPlayerState)
    {
        Timer = savedPlayerState.Timer;
        Health = savedPlayerState.Health;
        gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<ActiveCombatant>().Health = Health;
        balanceHealth();
        SuspiciousActivityCounter = savedPlayerState.SuspiciousActivityCounter;
        WantedLevel = calculateWantedLevel();
        _resetThreshold = TimeSpan.FromSeconds(calculateWantedLevelResetThreshold());
        Arrests = savedPlayerState.Arrests;
        Deaths = savedPlayerState.Deaths;
        Inventory.Items = savedPlayerState.Items;
        Inventory.Money = savedPlayerState.Money;
        WillShoot = savedPlayerState.WillShoot;
        AggroCooldown = savedPlayerState.AggroCooldown;
        Inventory.changeRecipe(gameManager.AssetManager.Images[savedPlayerState.RecipeInHud]);
    }
}
