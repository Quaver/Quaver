using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

// TODO decide if this should be included
public class SpriteProxy : DrawableProxy
{
    private readonly Sprite _container;

    [MoonSharpHidden]
    public SpriteProxy(Sprite container) : base(container)
    {
        _container = container;
    }
    
    public float Rotation
    {
        get => _container.Rotation;
        set => _container.Rotation = value;
    }

    public float Alpha
    {
        get => _container.Alpha;
        set => _container.Alpha = value;
    }

    public Color Tint
    {
        get => _container.Tint;
        set => _container.Tint = value;
    }
}