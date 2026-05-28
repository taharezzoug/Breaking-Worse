using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SoPra;

public class Game1 : Game
{
    // Logic objects
    private Vector2 LogoPos;
    private float timer = 0;
    private Vector2 screen_center;
    private float scale = 0.25f;
    
    // Game assets
    private SoundEffect hit;
    private SoundEffect miss;
    private Texture2D Background;
    private Texture2D UniLogo;
    
    // Game instances
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Initial Screen size is set to absolute resolution of Background image
        _graphics.PreferredBackBufferHeight = 1024;
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Sprites
        Background = Content.Load<Texture2D>("Background");
        UniLogo = Content.Load<Texture2D>("Unilogo");

        // Audio Effects
        hit = Content.Load<SoundEffect>("Logo_hit");
        miss = Content.Load<SoundEffect>("Logo_miss");

        // Screen
        Viewport viewport = _graphics.GraphicsDevice.Viewport;

        // Setting Logo starting Position
        screen_center.X = viewport.Width / 2;
        screen_center.Y = viewport.Height / 2;

        LogoPos.X = screen_center.X;
        LogoPos.Y = screen_center.Y - 300;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Total Elapsed time to calc Logo Position on Screen
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Add time elapsed since last update to total elapsed time
        timer += elapsed;
        // Logic for moving logo
        // Logo pos is calculated based on time as radian and a radius of 300px
        LogoPos.Y = (float)Math.Sin(timer) * 300 + screen_center.Y;
        LogoPos.X = (float)Math.Cos(timer) * 300 + screen_center.X;

        
        // Mouse click Sound events
        var mouseState = Mouse.GetState();
        var mousePosition = new Vector2(mouseState.X, mouseState.Y);

        // Mouse needs to be clicked and on Logo to play hit, other clicks play miss sound effect
        if (mouseState.LeftButton == ButtonState.Pressed ) {

            if (   mousePosition.X >= LogoPos.X - (UniLogo.Width * (scale * 0.5f)) 
                && mousePosition.X <= LogoPos.X + (UniLogo.Width * (scale * 0.5f)) 
                && mousePosition.Y >= LogoPos.Y - (UniLogo.Height * (scale * 0.5f)) 
                && mousePosition.Y <= LogoPos.Y + (UniLogo.Height * (scale * 0.5f))
                )   
                    {
                        hit.Play();
                    }

            else {
                miss.Play();
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Code for drawing Sprites onto screen
        _spriteBatch.Begin();
        _spriteBatch.Draw(Background, new Vector2(0,0), Color.White);
        _spriteBatch.Draw(UniLogo, LogoPos, null,  Color.White, 0f, new Vector2(UniLogo.Width / 2, UniLogo.Height / 2), scale, SpriteEffects.None, 0f);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
