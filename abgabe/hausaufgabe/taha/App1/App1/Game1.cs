using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace App1;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private Viewport _viewPort; 
    private SpriteBatch _spriteBatch;
    
    // Background variable
    private Texture2D _background;
    
    // Logo variables
    private Texture2D _uniLogo;
    private float _logoScale;
    private Vector2 _logoCoordinates;
    
    // Sound variables for hit/miss
    private SoundEffect _hit;
    private SoundEffect _miss;
    
    private float _timer;
    
    // Previous mouse state flag for sound on one click
    private bool _previousMouseState;
    
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // setting the resolution, the starting mouse stat, and the scale factor
        _graphics.PreferredBackBufferHeight = 1024;
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.ApplyChanges();
        _viewPort = _graphics.GraphicsDevice.Viewport;
        _previousMouseState = (Mouse.GetState().LeftButton == ButtonState.Pressed);
        _logoScale = 0.3f;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        // loading sprites and sounds
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _background = Content.Load<Texture2D>("Images/Background");
        _uniLogo = Content.Load<Texture2D>("Images/Unilogo");
        _miss = Content.Load<SoundEffect>("Sounds/Logo_miss");
        _hit = Content.Load<SoundEffect>("Sounds/Logo_hit");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        // calculate time in seconds since last frame
        // scale by 1.5 for faster animation speed
        var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * 1.5f;
        
        // calculate logos position depending on time
        _timer += elapsed;
        _logoCoordinates.X = (float)Math.Sin(_timer) * 280 + (float)_viewPort.Width / 2 - 150;
        _logoCoordinates.Y = (float)Math.Cos(_timer) * 280 + (float)_viewPort.Height / 2 - 150;
        
        var mouseState = Mouse.GetState();
        var mousePosX = mouseState.X;
        var mousePosY = mouseState.Y;
        
        // check if mouse position is within the screen bounds and if left mouse button was pressed
        if (mousePosX is >= 0 and <= 1280 &&
            mousePosY is >= 0 and <= 1024 &&
            mouseState.LeftButton == ButtonState.Pressed && !_previousMouseState)
        {
            // check if the mouse click is on logo and play appropriate sound
            if (mousePosX >= _logoCoordinates.X && mousePosX <= _logoCoordinates.X + 280
                                                && mousePosY >= _logoCoordinates.Y &&
                                                mousePosY <= _logoCoordinates.Y + 280)
            {

                _hit.Play();
            }
            else
            {
                _miss.Play();
            }
        }
        //update current mouse state to previous 
        _previousMouseState = mouseState.LeftButton == ButtonState.Pressed;
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkGray);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_background,new Vector2(0,0), Color.White); 
        _spriteBatch.Draw(_uniLogo, _logoCoordinates, null, Color.White, 0f, Vector2.Zero, _logoScale, SpriteEffects.None, 0f);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
