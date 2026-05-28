using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Aufgabe1;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    private Texture2D _backgroundTexture;
    private Vector2 _backgroundOrigin;

    private Texture2D _logoTexture;
    private Vector2 _logoPosition;
    
    private Vector2 _screenCenter;

    private SoundEffect _logoHit;
    private SoundEffect _logoMiss;
    
    //größe und rotationsparameter des Logos
    private float _rotationAngle = 0f;
    private float _rotationSpeed = 0.01f;
    private float _radius = 100f;
    private float _logoScale = 0.1f;
    private bool _buttonReleased = true;

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
        //laden des hintergrunds, logos und der sounds
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _backgroundTexture = Content.Load<Texture2D>("background");
        _backgroundOrigin = new Vector2(_backgroundTexture.Width / 2, _backgroundTexture.Height / 2);
        
        _logoTexture = Content.Load<Texture2D>("unilogo");
        _screenCenter = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        
        _logoHit = Content.Load<SoundEffect>("logo_hit");
        _logoMiss = Content.Load<SoundEffect>("logo_miss");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        //aktualisieren des Rotationswinkels
        _rotationAngle += _rotationSpeed;
        
        //aktualisieren der logo Position auf der Laufbahn eines Kreises
        _logoPosition = _screenCenter + new Vector2((float)Math.Cos(_rotationAngle) * _radius, (float)Math.Sin(_rotationAngle) * _radius);
        
        //logo als Rectangle deklarieren, wenn Maus bei klick innerhalb des Rectangles = logo hit, sonst logo miss
        MouseState mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed && _buttonReleased)
        {
            _buttonReleased = false;
            Rectangle _logoRectangle = new Rectangle(
                (int)(_logoPosition.X - _logoTexture.Width / 2 * _logoScale),
                (int)(_logoPosition.Y - _logoTexture.Height / 2 * _logoScale),
                (int)(_logoTexture.Width * _logoScale),
                (int)(_logoTexture.Height * _logoScale)
                );
            if (_logoRectangle.Contains(mouseState.Position))
            {
                _logoHit.Play();
            }
            else if(isMouseinWindow(mouseState.Position))
            {
                _logoMiss.Play();
            }
        }

        if (mouseState.LeftButton == ButtonState.Released)
        {
            _buttonReleased = true;
        }

        base.Update(gameTime);
    }

    private bool isMouseinWindow(Point mousePosition)
    {
        return GraphicsDevice.Viewport.Bounds.Contains(mousePosition);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        //hintergrund und logo drawen
        _spriteBatch.Begin();
        _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
        _spriteBatch.Draw(_logoTexture, _logoPosition, null, Color.White, 0f, new Vector2(_logoTexture.Width / 2f, _logoTexture.Height / 2f), _logoScale, SpriteEffects.None, 0f);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}