using System;
using System.Collections.Generic;
using breaking_worse.Objects.Hud;
using breaking_worse.Screens.Clickables;
using breaking_worse.Objects.Player;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.InGameScreens;

public class OnScreenDisplay : AScreen
{
    /// <summary>
    /// A simple On Screen Display class, that draws something on the Game Screen.
    /// </summary>

    private List<HudElement> _hudElements;
    private List<TextBox> _textBoxTechDemo;
    private Dictionary<Type, Button> _buttonsTechDemo;
    private ButtonCollection _dummyButtonCollection;
    private Button _blockButton;
    private readonly bool _toggleTechDemo;
    
    private IconTextHudElement _moneyHud;
    private float _moneyTextWidth;

    private IconTextHudElement _wantedTimerHud;
    private float _wantedTimerTextWidth;

    public OnScreenDisplay(GameManager gameManager) : base(gameManager, true, true)
    { 
        _moneyTextWidth = GameManager.AssetManager.Fonts["font"].MeasureString(GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money.ToString()).X * 0.1f * Scale;
        _wantedTimerTextWidth = GameManager.AssetManager.Fonts["font"].MeasureString(GameManager.ScreenManager.InGameScreen.PlayerState.RemainingWantedTime.ToString()).X * 0.1f * Scale;

        createHudElements();
    }

    private void createHudElements()
    {
        var timerHud = new IconTextHudElement(GameManager, new Vector2(850 * xScale, 20 * yScale), () => GameManager.ScreenManager.InGameScreen.PlayerState.Timer == int.MaxValue ? "-"
            : GameManager.ScreenManager.InGameScreen.PlayerState.Timer.ToString(), "stopwatch", 0.1f * Scale, new(75 * Scale, -5 * Scale));
        var fpsHud = new IconTextHudElement(GameManager, new Vector2(1250 * xScale, 20 * yScale), () => Math.Round(GameManager.Fps, 0).ToString(), "fps", 0.1f * Scale, new(75 * Scale, -5 * Scale));
        _moneyHud = new IconTextHudElement(GameManager, new Vector2(1850 * xScale - _moneyTextWidth / 2f, 20 * yScale), () => GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money.ToString(), "coin", 0.1f * Scale, new(75 * Scale, -5 * Scale));
        var wantedHud = new IconTextHudElement(GameManager, new Vector2(1800 * xScale, 90 * yScale), () => GameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel.ToString(), "wanted", 0.1f * Scale, new(75 * Scale, -5 * Scale));
        _wantedTimerHud = new IconTextHudElement(GameManager, new Vector2(1850 * xScale - _wantedTimerTextWidth / 2f, 160 * yScale), () => GameManager.ScreenManager.InGameScreen.PlayerState.RemainingWantedTime.ToString("F1"), "stopwatch", 0.1f * Scale, new(75 * Scale, -5 * Scale));
        var healthHud = new IconTextHudElement(GameManager, new Vector2(20 * xScale, 350 * yScale), () => GameManager.ScreenManager.InGameScreen.PlayerState.Health.ToString(), "health", 0.1f * Scale, new(75 * Scale, -5 * Scale));
        var weaponHudWalt = new SwitchWeaponHudElement(GameManager, GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt, GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.getBulletAmount);
        var weaponHudJesse = new SwitchWeaponHudElement(GameManager, GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse, GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.getBulletAmount);
        var cartelHud = new IconTextHudElement(GameManager, new Vector2(1825 * xScale, 250 * yScale), () => "", "CartelHud", 0.15f * Scale, new(75 * Scale, -5 * Scale), true);
        _hudElements = [fpsHud, _moneyHud, timerHud, healthHud, wantedHud, _wantedTimerHud, weaponHudWalt, weaponHudJesse, cartelHud];
    }

    private void initButtonsTechDemo()
    {
        _buttonsTechDemo = new Dictionary<Type, Button>();
        _dummyButtonCollection = new ButtonCollection(GameManager, Vector2.Zero, [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        
        int x = (int)(GameManager.SettingsManager.Resolution.Height - 100 * Scale);

        foreach (var element in GameManager.ScreenManager.InGameScreen.TechDemo.Tracker)
        {
            Rectangle rect = new Rectangle(10, x, (int)(40 * Scale), (int)(40 * Scale));
            var button = new Button(GameManager, _dummyButtonCollection, "", rect);
            _buttonsTechDemo.Add(element.Key, button);
            x -= (int)(50 * Scale);
        }
        _blockButton = new Button(GameManager, _dummyButtonCollection, "Block:B", new Rectangle(10, x, (int)(200 * Scale), (int)(40 * Scale))) 
            { Shade = Color.Red };
    }

    private void initTextTechDemo()
    {
        _textBoxTechDemo = new List<TextBox>();

        var x = 110 * Scale;
        
        foreach (var key in GameManager.ScreenManager.InGameScreen.TechDemo.Tracker.Keys)
        {
            var color = key == GameManager.ScreenManager.InGameScreen.TechDemo.LastSpawnType ? Color.LightGreen : Color.LightGray;           
            
            var textBox = new TextBox(
                GameManager,
                new Vector2(100 * Scale, GameManager.SettingsManager.Resolution.Height - x),
                $"{GameManager.ScreenManager.InGameScreen.TechDemo.Tracker[key].Item1} = {GameManager.ScreenManager.InGameScreen.TechDemo.Tracker[key].Item2}",
                GameManager.AssetManager.Fonts["numbers"],
                1f,
                color,
                false);
            x += 50 * Scale;
            
            _textBoxTechDemo.Add(textBox);

        }
        
        var textBoxSpawn = new TextBox(
            GameManager,
            new Vector2(10, GameManager.SettingsManager.Resolution.Height - (x + 50)),
            "Spawn:T",
            GameManager.AssetManager.Fonts["numbers"],
            1f,
            Color.Black,
            false
        );
        
        _textBoxTechDemo.Add(textBoxSpawn);
    }
    
    
    public override void draw(GameTime gameTime)
    {
        foreach (var hudElement in _hudElements)
            hudElement.draw();
        
        // draw recipes
        GameManager.SpriteBatch.Draw(GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.SelectedRecipe, new Rectangle((int)(GameManager.SettingsManager.Resolution.Width  - 232 * Scale), (int)(GameManager.SettingsManager.Resolution.Height /2f - 80 * Scale), (int)(192 * Scale), (int)(224 * Scale)), Color.White * 0.8f);

        if (ToggleTechDemo)
        {
            showExtraTechDemo();
            foreach (var textBox in _textBoxTechDemo)
                textBox.draw();
        }
    }

    private void showExtraTechDemo()
    {
        foreach (var button in _buttonsTechDemo)
        {
            button.Value.draw();
            button.Value.checkHovering();
            
            button.Value.Shade = (GameManager.ScreenManager.InGameScreen.TechDemo.LastSpawnType == button.Key)
                    ? Color.Green 
                    : Color.Red;            
            
            if (button.Value.isClicked())
            {
                GameManager.ScreenManager.InGameScreen.TechDemo.LastSpawnType = button.Key;
            }
            
            initTextTechDemo();
        }
        
        if (_blockButton.isClicked()) GameManager.ScreenManager.InGameScreen.TechDemo.BlockMode = !GameManager.ScreenManager.InGameScreen.TechDemo.BlockMode;
        _blockButton.Shade = GameManager.ScreenManager.InGameScreen.TechDemo.BlockMode ? Color.Green : Color.Red;

        _blockButton.draw();
        _blockButton.checkHovering();
    }
    
    public override void loadContent() {}
    public override void changeResolution()
    {
        xScale = GameManager.SettingsManager.Resolution.Width / 1920f;
        yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
        Scale = Math.Min(xScale, yScale);           
        _hudElements = [];
        createHudElements();
        if (ToggleTechDemo)
        {
            initButtonsTechDemo();
            initTextTechDemo();
        }
    }

    public override void update(GameTime gameTime)
    {
        foreach (var element in _hudElements)
            element.update();
            
        float newMoneyTextWidth = GameManager.AssetManager.Fonts["font"].MeasureString(GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money.ToString()).X * Scale;
        if (Math.Abs(_moneyTextWidth - newMoneyTextWidth) > 1)
        {
            _moneyTextWidth = newMoneyTextWidth;
            _moneyHud.newPosition = new Vector2(1850 * xScale - _moneyTextWidth / 2f, 20 * yScale);
        }
        
        float newWantedTimerTextWidth = GameManager.AssetManager.Fonts["font"].MeasureString(GameManager.ScreenManager.InGameScreen.PlayerState.RemainingWantedTime.ToString("F1")).X * Scale;
        if (Math.Abs(_wantedTimerTextWidth - newWantedTimerTextWidth) > 1)
        {
            _wantedTimerTextWidth = newWantedTimerTextWidth;
            _wantedTimerHud.newPosition = new Vector2(1850 * xScale - _wantedTimerTextWidth / 2f, 160 * yScale);
        }
    }
    
    public bool ToggleTechDemo
    {
        get => _toggleTechDemo;
        init
        {
            _toggleTechDemo = value;
            if (value)
            {
                initButtonsTechDemo();
            }
        }
    }
}
