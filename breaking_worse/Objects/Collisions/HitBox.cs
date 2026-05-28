using System;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Collisions;

public class HitBox
{
    // function that returns the position of the corresponding gameObject (including hitBox position offset)
    private readonly Func<Vector2> _getPosition;
    
    public Vector2 Position => _getPosition();
    
    public int Width { get; set; }
    public int Height { get; set; }

    public HitBox(Func<Vector2> getPosition)
    {
        _getPosition = getPosition;
    }

    public HitBox(Vector2 position, int width, int height)
    {
        _getPosition = () => position;
        Width = width;
        Height = height;
    }

    public bool intersects(HitBox hitBox)
    {
        return Position.X < hitBox.Position.X + hitBox.Width &&
               Position.X + Width > hitBox.Position.X &&
               Position.Y < hitBox.Position.Y + hitBox.Height &&
               Position.Y + Height > hitBox.Position.Y;
    }
    
    public HitBox getFutureHitBox(Vector2 direction)
    {
        return new HitBox(Position + direction, Width, Height);
    }

    public Vector2 CenterPoint => new(Position.X + Width / 2f, Position.Y + Height / 2f);

    public Rectangle convertToRectangle()
    {
        return new Rectangle(Position.ToPoint(), new Point(Width, Height));
    }
}