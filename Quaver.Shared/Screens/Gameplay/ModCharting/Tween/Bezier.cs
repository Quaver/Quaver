using System;
using System.Numerics;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

public class Bezier
{
    private static Vector2 Lerp(Vector2 from, Vector2 to, float t)
    {
        return from + (to - from) * t;
    }
    public static Vector2 CubicBezier(Vector2 from, Vector2 c1, Vector2 c2, Vector2 to, float t)
    {
        var p1 = Lerp(from, c1, t);
        var p2 = Lerp(c1, c2, t);
        var p3 = Lerp(c2, to, t);
        var p4 = Lerp(p1, p2, t);
        var p5 = Lerp(p2, p3, t);
        var p6 = Lerp(p4, p5, t);
        return p6;
    }
    public static Vector2 YFromT(Vector2 c1, Vector2 c2, float t)
    {
        return CubicBezier(Vector2.Zero, c1, c2, Vector2.One, t);
    }

    /// <summary>
    ///     https://stackoverflow.com/a/7355667/11225486 <br />
    ///     Solution for Bezier((0, 0), c1, c2, (1, 1)) for given x<br />
    ///     can be used for animation
    /// </summary>
    /// <param name="c1">First control point</param>
    /// <param name="c2">Second control point</param>
    /// <param name="targetX">X to look for</param>
    /// <returns>Y</returns>
    public static float YFromX(Vector2 c1, Vector2 c2, float targetX)
    {
        if (targetX < 0) return 0;
        if (targetX > 1) return 1;
        const float xTolerance = 0.001f; //adjust as you please
        var iterCount = 0;
        //we could do something less stupid, but since the x is monotonic
        //increasing given the problem constraints, we'll do a binary search.

        //establish bounds
        var lower = 0f;
        var upper = 1f;
        var percent = (upper + lower) / 2;

        //get initial x
        var x = YFromT(c1, c2, percent).X;

        //loop until completion
        while (MathF.Abs(targetX - x) > xTolerance)
        {
            if (targetX > x)
                lower = percent;
            else
                upper = percent;

            percent = (upper + lower) / 2;
            x = YFromT(c1, c2, percent).X;
            if (iterCount++ > 100) break;
        }

        //we're within tolerance of the desired x value.
        //return the y value.
        return YFromT(c1, c2, percent).Y;
    }
}