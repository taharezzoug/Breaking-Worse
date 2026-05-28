using System;
using breaking_worse.Screens;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Scene;

public class Camera(GameManager gameManager, InGameScreen inGameScreen)
{
    /// <summary>
    /// shows the objects and background on the map so that
    /// both the player characters fit into the frame if possible,
    /// has a maximum and minimum scale etc.
    /// </summary>
    
    // all the camera settings
    private const float MinScale = 0.5f;            // smaller values mean that cam zooms out further
    private const float MaxScale = 1.8f;            // bigger values mean that cam zoom in further
    private const float BufferZone = 240;           // distance to border of the screen before starting to scale

    private const float SpriteHeightOffSet = -32f;
    
    private const float ExactlyCameraZoomSpeed = 1.4f;      // factor to influence camera scaling speed
    private const float NotExactlyCameraZoomSpeed = 0.9f;   // don't touch this
    private const float CameraMoveSpeed = 1f / 16f;         // factor to influence camera movement speed
    
    // To zoom in, Jesse and Walt must be closer than a specified distance. This distance is different on the X and Y
    // axis, because of the aspect ratio (normally 16:9).
    // this factor indicates how much this is taken into account
    private const float ZoomInAspectRatioWeight = 1.3f;
    
    
    private Vector2 _centerPosition = new (gameManager.SettingsManager.Resolution.Width/2f, gameManager.SettingsManager.Resolution.Height/2f);                // cam position relative to origin
    public Vector2 Position { get; private set; }   // position of top left corner
    public float Scale { get; private set; } = 1;   // scaling the map to fit both players
    
    public Rectangle Bounds { get; private set; }
    
    private int _width = gameManager.SettingsManager.Resolution.Width;
    private int _height = gameManager.SettingsManager.Resolution.Height;
    
    private float _adjustedWidth = gameManager.SettingsManager.Resolution.Width / 2f - BufferZone;
    private float _adjustedHeight = gameManager.SettingsManager.Resolution.Height / 2f - BufferZone;
    private float _aspectRatio = gameManager.SettingsManager.Resolution.Height / (float)gameManager.SettingsManager.Resolution.Width * ZoomInAspectRatioWeight;
    

    public void update(GameTime gameTime)
    {
        var positionWalt = inGameScreen.GameObjectManager.Walt.Position;
        var positionJesse = inGameScreen.GameObjectManager.Jesse.Position;

        var scale = Scale;
        const int correction = 50000;
        
        // adjusts the scale down, zoom out
        Scale -= Math.Max(
            Math.Max(Math.Abs(positionWalt.X - positionJesse.X) * scale - 2f * _adjustedWidth, 0),
            Math.Max(Math.Abs(positionWalt.Y - positionJesse.Y) * scale - 2f * _adjustedHeight, 0)
            ) / correction * scale * ExactlyCameraZoomSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
        
        // adjusts the scale up, zoom in 
        Scale += Math.Max((float)(-Math.Sqrt(
                              Math.Pow(_aspectRatio * positionWalt.X - _aspectRatio * positionJesse.X, 2f) + 
                              Math.Pow(1 / _aspectRatio * positionWalt.Y - 1 / _aspectRatio * positionJesse.Y, 2f)) * scale + 
                                  NotExactlyCameraZoomSpeed * (_adjustedWidth + _adjustedHeight))
                          / correction, 0) * scale * ExactlyCameraZoomSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
        
        Scale = Math.Min(Scale, MaxScale);
        Scale = Math.Max(Scale, MinScale);
        
        var newPosition = _centerPosition;
        
        // measures the distance the character went beyond the screen boundary to the right
        var deviationWaltRight = positionWalt.X - (_centerPosition.X + _adjustedWidth / Scale);
        var deviationJesseRight = positionJesse.X - (_centerPosition.X + _adjustedWidth / Scale);
 
        newPosition.X += Math.Max(Math.Max(deviationWaltRight, 0), Math.Max(deviationJesseRight, 0)) * CameraMoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
        
        // measures the distance the character went beyond the screen boundary to the left
        var deviationWaltLeft = - positionWalt.X + (_centerPosition.X - _adjustedWidth / Scale);
        var deviationJesseLeft = - positionJesse.X + (_centerPosition.X - _adjustedWidth / Scale);
 
        newPosition.X -= Math.Max(Math.Max(deviationWaltLeft, 0), Math.Max(deviationJesseLeft, 0)) * CameraMoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60; 
        
        // measures the distance the character went beyond the screen boundary to the top
        var deviationWaltTop = - positionWalt.Y + (_centerPosition.Y - _adjustedHeight / Scale);
        var deviationJesseTop = - positionJesse.Y + (_centerPosition.Y - _adjustedHeight / Scale);
 
        newPosition.Y -= Math.Max(Math.Max(deviationWaltTop, 0), Math.Max(deviationJesseTop, 0)) * CameraMoveSpeed; 
        
        // measures the distance the character went beyond the screen boundary to the bottom
        var deviationWaltBottom = positionWalt.Y + Scale * SpriteHeightOffSet - (_centerPosition.Y + _adjustedHeight / Scale);
        var deviationJesseBottom = positionJesse.Y + Scale * SpriteHeightOffSet - (_centerPosition.Y + _adjustedHeight / Scale);
 
        newPosition.Y += Math.Max(Math.Max(deviationWaltBottom, 0), Math.Max(deviationJesseBottom, 0)) * CameraMoveSpeed;

        // ensure camera does not move beyond map borders
        newPosition.X = Math.Max(newPosition.X, _width / (2 * Scale));
        newPosition.X = Math.Min(newPosition.X, GameManager.MapWidth * GameManager.CellSize - _width / (2 * Scale));
        newPosition.Y = Math.Max(newPosition.Y, _height / (2 * Scale));
        newPosition.Y = Math.Min(newPosition.Y, GameManager.MapHeight * GameManager.CellSize - _height / (2 * Scale));
        
        _centerPosition = newPosition;
        Position = _centerPosition - new Vector2(_width / (2 * Scale), _height / (2 * Scale));
        
        Bounds = new Rectangle(Position.ToPoint(), new Vector2(_width/scale, _height/scale).ToPoint());
    }

    public void changeScreenResolution()
    {
        _width = gameManager.SettingsManager.Resolution.Width;
        _height = gameManager.SettingsManager.Resolution.Height;
        _adjustedWidth = _width / 2f - BufferZone;
        _adjustedHeight = _height / 2f - BufferZone;
        _aspectRatio = _height / (float)_width * ZoomInAspectRatioWeight;
    }
}
