using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Homework;

public class Game1 : Game
{
    private GraphicsDeviceManager mGraphics;
    private SpriteBatch mSpriteBatch;

    private Texture2D mBackgroundTexture;
    private Vector2 mBackgroundPos;
    private Vector2 mBackgroundScale;
    
    private Texture2D mLogoTexture;
    private Vector2 mLogoPos;
    private Vector2 mLogoScale;
    
    private float mRotationAngle;

    private Vector2 mCenter;

    private SoundEffect mLogoHit;
    private SoundEffect mLogoMiss;
    
    
    
    public Game1()
    {
        mGraphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferHeight = 600,
            PreferredBackBufferWidth = 900
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    
    protected override void Initialize()
    {
        //Setting Positions for Background and Logo and Scale Background
        mBackgroundPos = new Vector2(0f, 0f);
        mBackgroundScale = new Vector2(0.75f, 0.6f);
        
        mLogoPos = new Vector2(mGraphics.PreferredBackBufferWidth / 2f, mGraphics.PreferredBackBufferHeight / 2f);
        mLogoScale = new Vector2 (0.18f, 0.18f);
        
        mCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
        
        base.Initialize();
    }

    
    protected override void LoadContent()
    {
        mSpriteBatch = new SpriteBatch(GraphicsDevice);

        mBackgroundTexture = Content.Load<Texture2D>("Background");
        mLogoTexture = Content.Load<Texture2D>("Unilogo");

        mLogoHit = Content.Load<SoundEffect>("Logo_hit");
        mLogoMiss = Content.Load<SoundEffect>("Logo_miss");

    }
    
    
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        
        
        //Time since the Game started
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float radius = 100f;
        
        mRotationAngle += elapsed;
        
        //calculate new Position of Logo
        float x = mCenter.X + radius * (float)Math.Cos(mRotationAngle);
        float y = mCenter.Y + radius * (float)Math.Sin(mRotationAngle);
        mLogoPos = new Vector2(x, y);
        
        
        var mouseState = Mouse.GetState();

        //creates an invisible box around the Logo
        var box = new Rectangle(
            (int)(mLogoPos.X - mLogoTexture.Width * mLogoScale.X / 2f),
            (int)(mLogoPos.Y - mLogoTexture.Height * mLogoScale.Y/ 2f),
            (int)(mLogoTexture.Width * mLogoScale.X),
            (int)(mLogoTexture.Height * mLogoScale.Y));
        
        //check if Mouse is pressed, if True -> check if it's inside the box/Logo or outside
        //Sound is only available inside the monoGame window
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            if (box.Contains(mouseState.Position.ToVector2()))
            {
                mLogoHit.Play();
            }
            else if(mouseState.X > 0 && mouseState.X < mGraphics.PreferredBackBufferWidth && mouseState.Y > 0 && mouseState.Y < mGraphics.PreferredBackBufferHeight)
            {
                mLogoMiss.Play();
            }
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        mSpriteBatch.Begin(blendState: BlendState.Opaque);
        mSpriteBatch.Draw (mBackgroundTexture, mBackgroundPos, null, Color.White, 0f, Vector2.Zero,  mBackgroundScale, SpriteEffects.None, 0f);
        mSpriteBatch.End();

        mSpriteBatch.Begin(blendState: BlendState.AlphaBlend);
        mSpriteBatch.Draw(mLogoTexture, mLogoPos, null, Color.White, 0f, new Vector2(mLogoTexture.Width / 2f, mLogoTexture.Height / 2f), mLogoScale, SpriteEffects.None, 0f); 
        mSpriteBatch.End();
        
        
        base.Draw(gameTime);
    }
}