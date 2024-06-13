using System;
using System.Numerics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

[MoonSharpUserData]
public class TweenSetters
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut { get; }

    public TweenSetters(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
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

    public static SetterDelegate<Vector3> CreateVector3(Action<Vector3> action)
    {
        return (startValue, endValue, progress) => action(Vector3.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector3> CreateVector3(Closure action)
    {
        return (startValue, endValue, progress) => action?.SafeCall(Vector3.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector4> CreateVector4(Action<Vector4> action)
    {
        return (startValue, endValue, progress) => action(Vector4.Lerp(startValue, endValue, progress));
    }

    public static SetterDelegate<Vector4> CreateVector4(Closure action)
    {
        return (startValue, endValue, progress) => action?.SafeCall(Vector4.Lerp(startValue, endValue, progress));
    }

    public SetterDelegate<float> X(Drawable drawable)
    {
        return CreateFloat(v => drawable.X = v);
    }

    public SetterDelegate<float> Y(Drawable drawable)
    {
        return CreateFloat(v => drawable.Y = v);
    }

    public SetterDelegate<Vector2> Position(Drawable drawable)
    {
        return CreateVector2(v => drawable.Position = new ScalableVector2(v.X, v.Y));
    }

    public SetterDelegate<float> Rotation(Sprite sprite)
    {
        return CreateFloat(v => sprite.Rotation = v);
    }

    public SetterDelegate<float> Alpha(Sprite sprite)
    {
        return CreateFloat(v => sprite.Alpha = v);
    }

    public SetterDelegate<float> Width(Drawable drawable)
    {
        return CreateFloat(v => drawable.Width = v);
    }

    public SetterDelegate<float> Height(Drawable drawable)
    {
        return CreateFloat(v => drawable.Height = v);
    }

    public SetterDelegate<float> FontSize(SpriteTextPlus spriteTextPlus)
    {
        return CreateFloat(v => spriteTextPlus.FontSize = (int)v);
    }

    public SetterDelegate<float> HitObjectFallRotation(int lane)
    {
        return CreateFloat(v => Shortcut.GameplayPlayfieldKeys.HitObjectFallRotation[lane - 1] = v);
    }
}