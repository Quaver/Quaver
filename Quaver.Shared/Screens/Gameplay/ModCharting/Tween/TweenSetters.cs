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

    public static Action<int> CreateInt(Closure action)
    {
        return value => action?.SafeCall(value).ToObject<int>();
    }

    public static Action<float> CreateFloat(Closure action)
    {
        return value =>
            action?.SafeCall(value).ToObject<float>();
    }

    public static Action<Vector2> CreateVector2(Closure action)
    {
        return value => action?.SafeCall(value).ToObject<Vector2>();
    }

    public static Action<XnaVector2> CreateXnaVector2(Closure action)
    {
        return value => action?.SafeCall(value).ToObject<XnaVector2>();
    }

    public static Action<ScalableVector2> CreateScalableVector2(Closure action)
    {
        return value => action?.SafeCall(value).ToObject<ScalableVector2>();
    }

    public static Action<Vector3> CreateVector3(Closure action)
    {
        return value => action?.SafeCall(value).ToObject<Vector3>();
    }

    public static Action<Color> CreateColor(Closure action)
    {
        return value => action?.SafeCall(value).ToObject<Color>();
    }

    public static Action<Vector4> CreateVector4(Closure action)
    {
        return value => action?.SafeCall(value).ToObject<Vector4>();
    }
}