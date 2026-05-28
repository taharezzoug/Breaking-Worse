using System;
using Microsoft.Xna.Framework;

namespace breaking_worse.Utility;

public static class OwnMath
{
    public static int mod(int a, int m)
    {
        return (a % m + m) % m;
    }

    public static int angle(this Vector2 self, Vector2 other)
    {
        const int degree360 = 360;
        const int degree180 = 180;
        
        var dotProduct = Vector2.Dot(self, other);
        var magnitudeProduct = self.Length() * other.Length();
        
        var cosAlpha = dotProduct / magnitudeProduct;
        var angle = (int)Math.Round(MathHelper.ToDegrees((float)Math.Acos(cosAlpha)));
        
        // extend the angle from [0;180) to [0;360)
        var crossProduct = self.X * other.Y - self.Y * other.X;
        if (crossProduct < 0)
            angle = degree360 - angle;

        // adjust the angle to increase counterclockwise with zero on the right (like standard convention)
        angle = mod(degree180 - angle ,degree360);

        return angle;
    }
}
