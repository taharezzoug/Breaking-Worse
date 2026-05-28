using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Animations;

public class Animation
{
    public int Priority { get; }
    public bool IsActive { get; private set; }
    public bool RepeatLastFrame { get; }
    private readonly bool _shouldLoop;
    
    public Texture2D SpriteSheet { get; }
    private readonly List<Rectangle> _sourceRectangles;
    private readonly int _timePerRectangleInMilliSeconds;
    private double _elapsedTime;
    private int _currentRectangleIndex;
    public Rectangle CurrentRectangle => _sourceRectangles[_currentRectangleIndex];

    public Animation(Texture2D spriteSheet, int rectangleAmount, int rectangleWidth, int rectangleHeight, int timePerRectangleInMilliSeconds, int priority, bool shouldLoop, bool repeatLastFrame = false)
    {
        SpriteSheet = spriteSheet;
        _timePerRectangleInMilliSeconds = timePerRectangleInMilliSeconds;
        _sourceRectangles = [];
        _shouldLoop = shouldLoop;
        RepeatLastFrame = repeatLastFrame;
        Priority = priority;
        var rows = SpriteSheet.Height / rectangleHeight;
        var cols = SpriteSheet.Width / rectangleWidth;
        
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                if (row * cols + col >= rectangleAmount) break;
                _sourceRectangles.Add(new Rectangle(col * rectangleWidth, row * rectangleHeight, rectangleWidth, rectangleHeight));
            }
        }
    }
    
    public void update(GameTime gameTime)
    {
        if (!IsActive) return;

        _elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_elapsedTime >= _timePerRectangleInMilliSeconds)
        {
            _elapsedTime -= _timePerRectangleInMilliSeconds;
            _currentRectangleIndex++;
            if (_currentRectangleIndex >= _sourceRectangles.Count)
            {
                _currentRectangleIndex = 0;
                if (!_shouldLoop) stop();
            }
        }
    }

    public void start()
    {
        IsActive = true;
        _elapsedTime = 0.0;
        _currentRectangleIndex = 0;
    }

    public void stop()
    {
        IsActive = false;
        _elapsedTime = 0.0;
        _currentRectangleIndex = RepeatLastFrame ? _sourceRectangles.Count - 1 : 0;
    }
}
