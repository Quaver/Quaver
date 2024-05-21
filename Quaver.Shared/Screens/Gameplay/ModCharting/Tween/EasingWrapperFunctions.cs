using System.Numerics;
using MoonSharp.Interpreter;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

[MoonSharpUserData]
public class EasingWrapperFunctions
{
    public static readonly EasingDelegate Linear = progress => progress;

    public static EasingDelegate From(Easing easing) =>
        progress => EasingFunctions.Perform(easing, 0, 1, progress);
    public static EasingDelegate CubicBezier(Vector2 c1, Vector2 c2)
    {
        return progress => Bezier.YFromX(c1, c2, progress);
    }

    public static EasingDelegate Create(Closure closure) =>
        progress => closure?.SafeCall(progress)?.ToObject<float>() ?? 0;

    [MoonSharpUserDataMetamethod("__concat")]
    public static EasingDelegate Combine(EasingDelegate d1, EasingDelegate d2)
    {
        return progress => d1(d2(progress));
    }
}