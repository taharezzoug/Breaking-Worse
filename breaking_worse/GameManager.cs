using System;
using breaking_worse.AssetDistribution;
using breaking_worse.Input;
using breaking_worse.Screens;
using breaking_worse.Sound;
using breaking_worse.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse;

public class GameManager
{
    /// <summary>
    /// Loads, updates and draws all managers before giving it to the Main class
    /// </summary>
    
    // frames per second
    private const int TargetFps = 60;
    public float Fps { get; private set; }
    private DateTime _lastTime = DateTime.Now;

    // map dimensions
    public const int MapWidth = 250;
    public const int MapHeight = 140;
    public const int CellSize = 32;
    
    // monogame stuff
    public GraphicsDeviceManager GraphicsDeviceManager { get; }
    public Game Game { get; private set; }
    public ContentManager Content { get; private set; }
    public SpriteBatch SpriteBatch { get; private set; }
    
    // render targets
    public RenderTarget2D InGameScreenRenderTarget2DBase { get; private set; }
    public RenderTarget2D InGameScreenRenderTarget2DTop { get; private set; }
    public RenderTarget2D InGameScreenRenderTarget2D { get; private set; }
    public RenderTarget2D MiniMapRenderTarget2D { get; private set; }
    
    // own managers
    public InputHandler InputHandler { get; }
    public UserActionHandler UserActionHandler { get; }
    public AssetManager AssetManager { get; }
    public ScreenManager ScreenManager { get; }
    public SoundManager SoundManager { get; }
    public SaveManager SaveManager { get; }
    public SettingsManager SettingsManager { get; }
    public ProgressManager ProgressManager { get; }
    
    private float _elapsedTimeTotal;
    private int _frameCount;
    
    public GameManager(Game game, GraphicsDeviceManager graphicsDeviceManager)
    {
        Game = game;
        GraphicsDeviceManager = graphicsDeviceManager;
        
        // set target fps
        game.TargetElapsedTime = TimeSpan.FromSeconds(1d / TargetFps);
        
        // disable VSYNC (synchronization of rendered frames with monitor refresh rate)
        // reduces input lag
        graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
        
        // all Managers are initialized here (order matters)
        AssetManager = new AssetManager(this);
        InputHandler = new InputHandler();
        UserActionHandler = new UserActionHandler(this);
        SaveManager = new SaveManager();
        SoundManager = new SoundManager(this);
        SettingsManager = new SettingsManager(this);
        ProgressManager = new ProgressManager(this);
        ScreenManager = new ScreenManager(this);
    }

    public void loadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        Content = content;
        SpriteBatch = new SpriteBatch(graphicsDevice);
        InGameScreenRenderTarget2D = new RenderTarget2D(graphicsDevice, MapWidth * CellSize, MapHeight * CellSize);
        InGameScreenRenderTarget2DBase = new RenderTarget2D(graphicsDevice, MapWidth * CellSize, MapHeight * CellSize);
        InGameScreenRenderTarget2DTop = new RenderTarget2D(graphicsDevice, MapWidth * CellSize, MapHeight * CellSize);
        MiniMapRenderTarget2D = new RenderTarget2D(graphicsDevice, MapWidth * CellSize, MapHeight * CellSize);
        
        // load assets here
        AssetManager.loadContent();
        SoundManager.loadContent();

        ScreenManager.initialize();
    }

    public void update(GameTime gameTime)
    {
        InputHandler.update(gameTime);
        UserActionHandler.update();
        ScreenManager.update(gameTime);
    }

    public void draw(GameTime gameTime)
    { 
        calculateFramesPerSecond();

        ScreenManager.renderInGameScreen(gameTime);
        
        SpriteBatch.Begin();
        ScreenManager.draw(gameTime);
        SpriteBatch.End();
    }

    // Averages the FPS for a smoother update
    private void calculateFramesPerSecond()
    {
        _frameCount++;
        var currentTime = DateTime.Now;
        var elapsedTime = (currentTime - _lastTime).TotalSeconds;
        _lastTime = currentTime;

        _elapsedTimeTotal += (float)elapsedTime;

        if (_elapsedTimeTotal >= 1.0f)
        {
            Fps = _frameCount / _elapsedTimeTotal;
            _frameCount = 0;
            _elapsedTimeTotal = 0;
        }
    }
}
