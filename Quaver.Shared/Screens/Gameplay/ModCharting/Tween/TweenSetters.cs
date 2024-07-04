using System;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

[MoonSharpUserData]
public class TweenSetters
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut { get; }

    public TweenSetters(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
    }

    public static SetterDelegate<T> CreateSwap<T>(SetterDelegate<T> a, SetterDelegate<T> b)
    {
        return (startValue, endValue, progress) =>
        {
            a(startValue, endValue, progress);
            b(endValue, startValue, progress);
        };
    }

    public static SetterDelegate<int> CreateInt(Action<int> action)
    {
        return (startValue, endValue, progress) => action((int)EasingFunctions.Linear(startValue, endValue, progress));
    }

    public static SetterDelegate<int> CreateInt(Closure action)
    {
        return (startValue, endValue, progress) =>
            action?.SafeCall(EasingFunctions.Linear(startValue, endValue, progress));
    }

    public static SetterDelegate<float> CreateFloat(Action<float> action)
    {
        return (startValue, endValue, progress) => action(EasingFunctions.Linear(startValue, endValue, progress));
    }

    public static SetterDelegate<float> CreateFloat(Closure action)
    {
        return (startValue, endValue, progress) =>
            action?.SafeCall(EasingFunctions.Linear(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector2> CreateVector2(Action<Vector2> action)
    {
        return (startValue, endValue, progress) => action(Vector2.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector2> CreateVector2(Closure action)
    {
        return (startValue, endValue, progress) => action?.SafeCall(Vector2.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<XnaVector2> CreateXnaVector2(Action<XnaVector2> action)
    {
        return (startValue, endValue, progress) => action(XnaVector2.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<XnaVector2> CreateXnaVector2(Closure action)
    {
        return (startValue, endValue, progress) => action?.SafeCall(XnaVector2.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<ScalableVector2> CreateScalableVector2(Action<ScalableVector2> action)
    {
        return (startValue, endValue, progress) => action(ScalableVector2.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<ScalableVector2> CreateScalableVector2(Closure action)
    {
        return (startValue, endValue, progress) => action?.SafeCall(ScalableVector2.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector3> CreateVector3(Action<Vector3> action)
    {
        return (startValue, endValue, progress) => action(Vector3.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector3> CreateVector3(Closure action)
    {
        return (startValue, endValue, progress) => action?.SafeCall(Vector3.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Color> CreateColor(Action<Color> action)
    {
        return (startValue, endValue, progress) => action(Color.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Color> CreateColor(Closure action)
    {
        return (startValue, endValue, progress) => action?.SafeCall(Color.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector4> CreateVector4(Action<Vector4> action)
    {
        return (startValue, endValue, progress) => action(Vector4.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector4> CreateVector4(Closure action)
    {
        return (startValue, endValue, progress) => action?.SafeCall(Vector4.Lerp(startValue, endValue, progress));
    }
}