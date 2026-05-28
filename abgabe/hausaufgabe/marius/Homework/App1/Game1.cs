// I wrote this code blind, as I don't get monogame to work on my macOS,
// Linux and even my old Windows PC, so no idea how well it works or if at all.

using System;
using System.ComponentModel.Design;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace App1;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    // Images
    private Texture2D _wallpaperImage;
    private Texture2D _logoImage;
    // Sounds
    private SoundEffect _clickSound;
    private SoundEffect _missSound;
    // POsitions
    private Vector2 _logoPosition;
    private Vector2 _wallpaperPosition;
    private Vector2 _windowPosition;
    private float _logoAngle = 0f;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _wallpaperImage = Content.Load<Texture2D>("Background");
        _logoImage = Content.Load<Texture2D>("Unilogo");
        _clickSound = Content.Load<SoundEffect>("Logo_hit");
        _missSound = Content.Load<SoundEffect>("Logo_miss");
        
        _wallpaperPosition = new Vector2(_wallpaperImage.Width/2, _wallpaperImage.Height/2);
        _windowPosition = new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height/2);

    }

    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        // Rotating
        _logoAngle += 0.1f;
        _logoPosition = _windowPosition + new Vector2((float)Math.Cos(_logoAngle) * 10f, (float)Math.Sin(_logoAngle) * 10f);
        
        MouseState mouse = Mouse.GetState();
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            float _mouseDistanceX = ((_logoPosition.X - MouseState.Position.X)*(_logoPosition.X - MouseState.Position.X));
            float _mouseDistanceY = ((_logoPosition.Y - MouseState.Position.Y)*(_logoPosition.Y - MouseState.Position.Y));
            if ((_mouseDistanceX + _mouseDistanceY) < 10)
            {
                _clickSound.Play();
            }
            else
            {
                _missSound.Play();
            }
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // TODO: Add your drawing code here
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_wallpaperImage, new Vector2(0, 0), Color.White);
        _spriteBatch.Draw(_logoImage, _logoPosition, Color.White);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}