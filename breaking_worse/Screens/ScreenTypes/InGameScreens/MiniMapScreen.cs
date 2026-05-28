using System;
using System.Linq;
using breaking_worse.Objects;
using breaking_worse.Objects.Combat;
using breaking_worse.Objects.Enemy;
using breaking_worse.Objects.Interaction;
using breaking_worse.Objects.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.ScreenTypes.InGameScreens;

public class MiniMapScreen : AScreen
{
    /// <summary>
    /// Mini Map draws a simplified mini map on the Screen, that contains arrows for the Player Characters,
    /// FOV circles for the Enemies (different colors for Police/Cartel) and colored dots for things that is dialogable in the game.
    /// Position and dimensions can be adjusted as well as the size of the excerpt of the map that is shown.
    /// </summary>

    private int _positionX;
    private int _positionY;
    private int _width;
    private int _height;
    private readonly float _opacity;
    private readonly Texture2D _texture;
    
    // Colors for:
    // Walt arrow
    private readonly Color _colorWalt = Color.Green;
    // Jesse arrow
    private readonly Color _colorJesse = Color.Yellow;
    // police FOV circles
    private readonly Color _colorPolice = Color.Blue;
    // cartel FOV circles
    private readonly Color _colorCartel = Color.Red;
    // dots for dialogables
    private readonly Color _colorDialogables = Color.White;
    // color for buyer bubbles
    private readonly Color _colorBuyers = Color.DarkOrange;
    // color for buyer bubbles
    private readonly Color _colorSellers = Color.Green;
    
    // FOV size for enemy
    private float _fovSizePolice = 60f;
    private readonly float _fovSizeCartel = 320f;
    
    // camera bounds on in game screen
    private Rectangle _cameraRectangle;

    public MiniMapScreen(GameManager gameManager) : base(gameManager, true, true)
    {
        _positionX = (int)(15 * xScale);
        _positionY = (int)(15 * yScale);
        _width = (int)(500 * Scale);
        _height = (int)(300 * Scale);
        _opacity = 0.9f;
        _texture = new(gameManager.GraphicsDeviceManager.GraphicsDevice, 1, 1);
        _texture.SetData([Color.Black]);
    }

  
    
    public override void update(GameTime gameTime)
    {
        _cameraRectangle = GameManager.ScreenManager.InGameScreen.Camera.Bounds;
        int addedWidth =  5000 - _cameraRectangle.Width;
        int addedHeight = 3000 -_cameraRectangle.Height;
        _cameraRectangle.Inflate(addedWidth / 2, addedHeight / 2);
        
        // so map does not go beyond map boundaries (on normal screen sizes):
        _cameraRectangle.Location = new Point(Math.Max(0, _cameraRectangle.Location.X), Math.Max(0, _cameraRectangle.Location.Y));
        _cameraRectangle.Location = new Point(Math.Min(GameManager.ScreenManager.InGameScreen.Map.MapWidth*32 - _cameraRectangle.Width, _cameraRectangle.Location.X),
                                              Math.Min(GameManager.ScreenManager.InGameScreen.Map.MapHeight*32 - _cameraRectangle.Height, _cameraRectangle.Location.Y));
        _fovSizePolice = Math.Max(0.2f, GameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel) * 400 * 2;
        // times 400 because that's alert radius, times 2 because radius not diameter and minimum:=0.2 to still see something with wanted level 0
    }


    
    public override void draw(GameTime gameTime)
    {
        InGameScreen igs = GameManager.ScreenManager.InGameScreen;
        
        // draw background color to make the map easier visible
        GameManager.SpriteBatch.Draw(_texture,
                                     new Rectangle(_positionX, _positionY, _width, _height),
                                     Color.White*0.4f);
        // draw map
        GameManager.SpriteBatch.Draw(GameManager.MiniMapRenderTarget2D,
                                     new Rectangle(_positionX, _positionY, _width, _height),
                                     _cameraRectangle,
                                     Color.White * _opacity);
        // draw Walt arrow
        GameManager.SpriteBatch.Draw(GameManager.AssetManager.Images["miniMapLocationIndicator"],
                (igs.GameObjectManager.Walt.Position - _cameraRectangle.Location.ToVector2())/
                            _cameraRectangle.Size.ToVector2() * 
                                new Vector2(_width ,_height) + 
                                    new Vector2(_positionX, _positionY),
                null,
                _colorWalt * _opacity,
                (float)Math.Atan2(igs.GameObjectManager.Walt.DirectionFov.Y, igs.GameObjectManager.Walt.DirectionFov.X) + (float)Math.PI/4f,
                new Vector2(32, 32),
                0.5f,
                SpriteEffects.None,
                0);
        
        // draw Jesse arrow
        GameManager.SpriteBatch.Draw(GameManager.AssetManager.Images["miniMapLocationIndicator"],
                (igs.GameObjectManager.Jesse.Position - _cameraRectangle.Location.ToVector2())/
                            _cameraRectangle.Size.ToVector2() * 
                                new Vector2(_width ,_height) + 
                                    new Vector2(_positionX, _positionY),
                null,
                _colorJesse * _opacity,
                (float)Math.Atan2(igs.GameObjectManager.Jesse.DirectionFov.Y, igs.GameObjectManager.Jesse.DirectionFov.X) + (float)Math.PI/4f,
                new Vector2(32, 32),
                0.5f,
                SpriteEffects.None,
                0);
        
        // draw the FOV circles for Police / Cartel
        foreach (var gameObject in 
                 igs.GameObjectManager.getObjectsThatHaveComponent<ActiveCombatant>().Where(gameObject => 
                     gameObject is not PlayerCharacter && _cameraRectangle.Contains(gameObject.Position)))
        {
            Vector2 gameObjectPositionInMiniMap = (gameObject.Position - _cameraRectangle.Location.ToVector2()) / _cameraRectangle.Size.ToVector2() * new Vector2(_width ,_height);

            float scale = (gameObject is Police ? _fovSizePolice : _fovSizeCartel) * (_width / (float)_cameraRectangle.Size.X) / 64; // divided by 64 because the image is 64x64;

            float offsetX = 0;
            float offsetY = 0;
            offsetX += scale*32 - Math.Min(scale*32, gameObjectPositionInMiniMap.X);
            offsetY += scale*32 - Math.Min(scale*32, gameObjectPositionInMiniMap.Y);
            offsetX += _width-scale*32 - Math.Max(_width - scale*32, gameObjectPositionInMiniMap.X);
            offsetY += _height-scale*32 - Math.Max(_height - scale*32, gameObjectPositionInMiniMap.Y);
            
            
            GameManager.SpriteBatch.Draw(GameManager.AssetManager.Images["FOVIndicatorCircleWhite"],
                gameObjectPositionInMiniMap + new Vector2(_positionX, _positionY) + new Vector2(offsetX, offsetY),
                new Rectangle((int)offsetX, (int)offsetY, 64, 64),
                gameObject is Police ? _colorPolice * _opacity * (float)(0.2 + 100/_fovSizePolice) : _colorCartel * _opacity * 0.5f,
                0f,
                new Vector2(32, 32),
                scale,
                SpriteEffects.None,
                0);
        }
        
        // draw dots for everything that is dialogable
        foreach (var gameObject in 
                 igs.GameObjectManager.getObjectsThatHaveComponent<Dialogable>().Where(gameObject => 
                     gameObject is not PlayerCharacter && _cameraRectangle.Contains(gameObject.Position) && gameObject.Position.Length() > 1))
        {
            Color color = _colorDialogables;
            if (gameObject is NpcBuyer) color = _colorBuyers;
            if (gameObject is Npc) color = _colorSellers;
            
            GameManager.SpriteBatch.Draw(GameManager.AssetManager.Images["dialogableBubble"],
                (gameObject.Position - _cameraRectangle.Location.ToVector2())/
                            _cameraRectangle.Size.ToVector2() * 
                                new Vector2(_width ,_height) + 
                                    new Vector2(_positionX, _positionY),
                null,
                color * _opacity,
                0f,
                new Vector2(32, 32),
                0.3f,
                SpriteEffects.None,
                0);
        }
    }
    
    public override void loadContent()
    {
        // nothing to load
    }

    public override void changeResolution()
    {
        xScale = GameManager.SettingsManager.Resolution.Width / 1920f;
        yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
        Scale = Math.Min(xScale, yScale);               
        _positionX = (int)(15 * xScale);
        _positionY = (int)(15 * yScale);
        _width = (int)(500 * Scale);
        _height = (int)(300 * Scale);
    }
}
