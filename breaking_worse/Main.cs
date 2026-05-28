using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse;

public class Main : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private GameManager _gameManager;
    private float _timeOfLastEvent;
    
    public Main()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        
        // better performance while switching screen resolutions
        _graphics.HardwareModeSwitch = false;
        // subscribed to event for antialiasing
        _graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
    }
    
    // anti aliasing
    private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
    {
        const int multiSampleCount = 9;
        _graphics.PreferMultiSampling = true;
        e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = multiSampleCount; 
    } 
    
    protected override void Initialize()
    {
        Window.AllowUserResizing = false;
        Window.ClientSizeChanged += OnResize;
        _graphics.ApplyChanges();
        
        _gameManager = new GameManager(this, _graphics);
        IsMouseVisible = true;
        
        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        _gameManager.loadContent(Content, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        _gameManager.update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Transparent);

        _gameManager.draw(gameTime);
        base.Draw(gameTime);
        if (gameTime.ElapsedGameTime.TotalMilliseconds > 1000 / 60f)
        {
            _timeOfLastEvent = (float)gameTime.TotalGameTime.TotalMilliseconds;
        }
    }
    
    private void OnResize(object sender, EventArgs e)
    {
        // when the actual window size doesn't match the settings, change it
        if (_graphics.PreferredBackBufferWidth != _graphics.GraphicsDevice.Viewport.Width ||
            _graphics.PreferredBackBufferHeight != _graphics.GraphicsDevice.Viewport.Height)
        {
            _gameManager.SettingsManager.changeResolution(_graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);
        }
    }
}
