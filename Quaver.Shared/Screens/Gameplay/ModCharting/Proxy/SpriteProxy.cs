using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class SpriteProxy : DrawableProxy
{
    private readonly Sprite _drawable;

    [MoonSharpHidden]
    public SpriteProxy(Sprite drawable) : base(drawable)
    {
        _drawable = drawable;
    }

    public float Alpha
    {
        get => _drawable.Alpha;
        set => _drawable.Alpha = value;
    }

    public Color Tint
    {
        get => _drawable.Tint;
        set => _drawable.Tint = value;
    }

    public bool IndependentRotation
    {
        get => _drawable.IndependentRotation;
        set => _drawable.IndependentRotation = value;
    }
}