using System.Numerics;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

public static class EasingWrapperFunctions
{
    public static readonly EasingDelegate Linear = progress => progress;

    public static EasingDelegate From(Easing easing) =>
        progress => EasingFunctions.Perform(easing, 0, 1, progress);
    public static EasingDelegate CubicBezier(Vector2 c1, Vector2 c2)
    {
        return progress => Bezier.YFromX(c1, c2, progress);
    }
}