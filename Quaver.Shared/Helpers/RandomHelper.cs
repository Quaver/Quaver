using System;
using MonoGame.Extended;

namespace Quaver.Shared.Helpers;

using XnaVector2 = Microsoft.Xna.Framework.Vector2;

public static class RandomHelper
{
    /// <summary>
    /// </summary>
    private static readonly FastRandom random = new();

    /// <summary>
    ///     Random number [0, 1]
    /// </summary>
    /// <returns></returns>
    public static float RandomUniform() => random.NextSingle();

    /// <summary>
    ///     Randomly returns 0 or 1
    /// </summary>
    /// <returns></returns>
    public static int RandomBinary() => random.Next(0, 1);

    /// <summary>
    ///     Returns a random unit vector in R^2
    /// </summary>
    /// <returns></returns>
    public static XnaVector2 RandomUnitXnaVector2()
    {
        random.NextUnitVector(out var vec);
        return vec;
    }

    /// <summary>
    ///     Returns a random number following gaussian (normal) distribution N(<see cref="mean"/>, <see cref="stdDev"/>^2)
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="stdDev"></param>
    /// <returns></returns>
    public static float RandomGauss(float mean = 0, float stdDev = 1)
    {
        var u1 = 1 - random.NextSingle();
        var u2 = 1 - random.NextSingle();
        var randStdNormal = MathF.Sqrt(-2 * MathF.Log(u1)) * MathF.Sin(2 * MathF.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}