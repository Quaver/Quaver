using System.Numerics;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Tween;

public static class EasingWrapperFunctions
{
    public static TweenPayload.EasingDelegate Linear => EasingFunctions.Linear;
    public static TweenPayload.EasingDelegate FromEasing(Easing easing)
    {
        return (start, end, progress) => EasingFunctions.Perform(easing, start, end, progress);
    }

    public static TweenPayload.EasingDelegate CubicBezier(Vector2 c1, Vector2 c2)
    {
        return (start, end, progress) => Linear(start, end, Bezier.YFromX(c1, c2, progress));
    }
}