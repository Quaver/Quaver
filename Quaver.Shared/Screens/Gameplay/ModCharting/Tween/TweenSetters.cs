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

    public static TweenPayload<float>.SetterDelegate CreateFloat(Action<float> action)
    {
        return (startValue, endValue, progress) => action(EasingFunctions.Linear(startValue, endValue, progress));
    }
    
    public static TweenPayload<Vector2>.SetterDelegate CreateVector2(Action<Vector2> action)
    {
        return (startValue, endValue, progress) => action(Vector2.Lerp(startValue, endValue, progress));
    }
    public static TweenPayload<Vector3>.SetterDelegate CreateVector3(Action<Vector3> action)
    {
        return (startValue, endValue, progress) => action(Vector3.Lerp(startValue, endValue, progress));
    }
    public static TweenPayload<Vector4>.SetterDelegate CreateVector4(Action<Vector4> action)
    {
        return (startValue, endValue, progress) => action(Vector4.Lerp(startValue, endValue, progress));
    }

    public TweenPayload<float>.SetterDelegate X(Drawable drawable)
    {
        return CreateFloat(v => drawable.X = v);
    }
    public TweenPayload<float>.SetterDelegate Y(Drawable drawable)
    {
        return CreateFloat(v => drawable.Y = v);
    }
    public TweenPayload<Vector2>.SetterDelegate Position(Drawable drawable)
    {
        return CreateVector2(v => drawable.Position = new ScalableVector2(v.X, v.Y));
    }
    public TweenPayload<float>.SetterDelegate Rotation(Sprite sprite)
    {
        return CreateFloat(v => sprite.Rotation = v);
    }
    
    public TweenPayload<float>.SetterDelegate Alpha(Sprite sprite)
    {
        return CreateFloat(v => sprite.Alpha = v);
    }
    
    public TweenPayload<float>.SetterDelegate Width(Drawable drawable)
    {
        return CreateFloat(v => drawable.Width = v);
    }

    public TweenPayload<float>.SetterDelegate Height(Drawable drawable)
    {
        return CreateFloat(v => drawable.Height = v);
    }

    public TweenPayload<float>.SetterDelegate FontSize(SpriteTextPlus spriteTextPlus)
    {
        return CreateFloat(v => spriteTextPlus.FontSize = (int)v);
    }

    public TweenPayload<float>.SetterDelegate HitObjectFallRotation(int lane)
    {
        return CreateFloat(v => Shortcut.GameplayPlayfieldKeys.HitObjectFallRotation[lane - 1] = v);
    }
}