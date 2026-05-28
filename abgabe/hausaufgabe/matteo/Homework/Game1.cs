using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Homework;

public class Game1 : Game
{
    // game assets
    private Texture2D mBackgroundTexture;
    private Texture2D mLogoTexture;
    private SoundEffect mLogoHitSound;
    private SoundEffect mLogoMissSound;
    
    // asset attributes
    private Vector2 mBackgroundPosition;
    private Vector2 mBackgroundScale;
    private Vector2 mLogoPosition;
    private Vector2 mLogoScale;

    // x and y coordinates of the screen center
    // stored here so that they don't have to be recalculated every frame
    private float mScreenCenterX;
    private float mScreenCenterY;

    // logo rotation behavior
    private const float LogoSpeedInDegreesPerSecond = 120f;
    private const float RadiusInPixels = 250f;

    // helper variable for mouse input
    // needed so that each click only triggers one sound
    private bool mWasMousePressed;

    // monogame variables
    private readonly GraphicsDeviceManager mGraphics;
    private SpriteBatch mSpriteBatch;

    public Game1()
    {
        mGraphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // set window resolution (matches with .75 scaling of the background)
        mGraphics.PreferredBackBufferWidth = 960;
        mGraphics.PreferredBackBufferHeight = 768;
        mGraphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // set asset positions and scales
        mBackgroundPosition = new Vector2(0f,0f);
        mBackgroundScale = new Vector2(0.75f, 0.75f);
        mLogoPosition = new Vector2();
        mLogoScale = new Vector2(0.15f, 0.15f);

        mScreenCenterX = mGraphics.PreferredBackBufferWidth / 2f;
        mScreenCenterY = mGraphics.PreferredBackBufferHeight / 2f;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        mSpriteBatch = new SpriteBatch(GraphicsDevice);

        // textures
        mLogoTexture = Content.Load<Texture2D>("images/unilogo");
        mBackgroundTexture = Content.Load<Texture2D>("images/background");
        
        // sound effects
        mLogoHitSound = Content.Load<SoundEffect>("sounds/logo_hit");
        mLogoMissSound = Content.Load<SoundEffect>("sounds/logo_miss");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // total time since the game started
        var totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

        /*
         * position of the logo is calculated using sin and cos functions
         * Arguments of sin and cos:
         * - totalTime constantly increases, therefore the logo moves in a circle
         * - multiplied by LogoSpeed and (π/180), so that when LogoSpeed = 1, it rotates 1 degree per second
         */
        mLogoPosition.X = mScreenCenterX + RadiusInPixels * (float)Math.Cos(totalTime * (float.Pi / 180f) * LogoSpeedInDegreesPerSecond);
        mLogoPosition.Y = mScreenCenterY + RadiusInPixels * (float)Math.Sin(totalTime * (float.Pi / 180f) * LogoSpeedInDegreesPerSecond);

        // check if window is focused
        if (IsActive)
        {
            var mouseState = Mouse.GetState();
            var mousePosition = mouseState.Position.ToVector2();
            var isMousePressed = mouseState.LeftButton == ButtonState.Pressed;
            
            // check for mouse click and if the mouse is inside the window
            if (isMousePressed && !mWasMousePressed &&
                mousePosition.X >= 0 && mousePosition.X <= mGraphics.PreferredBackBufferWidth &&
                mousePosition.Y >= 0 && mousePosition.Y <= mGraphics.PreferredBackBufferHeight)
            {
                
                // hitbox of the logo
                // x and y coordinates are calculated so that the hitbox is centered on the logo
                var logoHitbox = new Rectangle(
                    (int)(mLogoPosition.X - mLogoTexture.Width * mLogoScale.X / 2f),
                    (int)(mLogoPosition.Y - mLogoTexture.Height * mLogoScale.Y / 2f),
                    (int)(mLogoTexture.Width * mLogoScale.X),
                    (int)(mLogoTexture.Height * mLogoScale.Y)
                );

                if (logoHitbox.Contains(mousePosition))
                {
                    mLogoHitSound.Play();
                }
                else
                {
                    mLogoMissSound.Play();
                }
            }
            
            mWasMousePressed = isMousePressed;
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        mSpriteBatch.Begin();
        mSpriteBatch.Draw(mBackgroundTexture, mBackgroundPosition, null, Color.White, 0f, Vector2.Zero, mBackgroundScale, SpriteEffects.None, 0f);
        mSpriteBatch.Draw(mLogoTexture, mLogoPosition, null, Color.White, 0f, new Vector2(mLogoTexture.Width / 2f, mLogoTexture.Height / 2f), mLogoScale, SpriteEffects.None, 0f);
        mSpriteBatch.End();

        base.Draw(gameTime);
    }
}
