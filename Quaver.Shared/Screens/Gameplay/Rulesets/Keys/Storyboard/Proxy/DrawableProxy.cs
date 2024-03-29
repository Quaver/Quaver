using System.Collections.Generic;
using System.Numerics;
using MoonSharp.Interpreter;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

// TODO decide if this should be included
public class DrawableProxy
{
    private readonly Drawable _drawable;

    [MoonSharpHidden]
    public DrawableProxy(Drawable drawable)
    {
        _drawable = drawable;
    }

    public Drawable Place(ScalableVector2 coords)
    {
        Position = coords;
        return _drawable;
    }

    public Drawable Place(Vector2 coords)
    {
        Position = new ScalableVector2(coords.X, coords.Y);
        return _drawable;
    }

    public Drawable Move(Vector2 coords)
    {
        Position = new ScalableVector2(Position.X.Value + coords.X, Position.Y.Value + coords.Y, Position.X.Scale,
            Position.Y.Scale);
        return _drawable;
    }

    public Drawable Scale(float factor)
    {
        Size = new ScalableVector2(Size.X.Value * factor, Size.Y.Value * factor, Size.X.Scale, Size.Y.Scale);
        return _drawable;
    }

    public Drawable Scale(Vector2 factor)
    {
        Size = new ScalableVector2(Size.X.Value * factor.X, Size.Y.Value * factor.Y, Size.X.Scale, Size.Y.Scale);
        return _drawable;
    }

    public Drawable WithParent(Drawable parent)
    {
        Parent = parent;
        return _drawable;
    }

    public Drawable Align(Alignment alignment)
    {
        Alignment = alignment;
        return _drawable;
    }

    public ScalableVector2 Position
    {
        get => _drawable.Position;
        set => _drawable.Position = value;
    }

    public ScalableVector2 Size
    {
        get => _drawable.Size;
        set => _drawable.Size = value;
    }

    public Drawable Parent
    {
        get => _drawable.Parent;
        set => _drawable.Parent = value;
    }

    public Alignment Alignment
    {
        get => _drawable.Alignment;
        set => _drawable.Alignment = value;
    }

    public List<Drawable> Children => _drawable.Children;
}