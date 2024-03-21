using System.Collections.Generic;
using MoonSharp.Interpreter;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

// TODO decide if this should be included
public class DrawableProxy
{
    private readonly Drawable _container;

    [MoonSharpHidden]
    public DrawableProxy(Drawable container)
    {
        _container = container;
    }

    public ScalableVector2 Position
    {
        get => _container.Position;
        set => _container.Position = value;
    }

    public ScalableVector2 Size
    {
        get => _container.Size;
        set => _container.Size = value;
    }

    public Drawable Parent
    {
        get => _container.Parent;
        set => _container.Parent = value;
    }

    public List<Drawable> Children => _container.Children;
}