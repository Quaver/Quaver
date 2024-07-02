using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Wobble.Graphics;
using Vector2 = System.Numerics.Vector2;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class DrawableProxy
{
    private readonly Drawable _drawable;

    [MoonSharpHidden]
    public DrawableProxy(Drawable drawable)
    {
        _drawable = drawable;
    }

    public Drawable MoveTo(ScalableVector2 coords)
    {
        Position = coords;
        return _drawable;
    }

    public Drawable MoveTo(Vector2 coords)
    {
        Position = new ScalableVector2(coords.X, coords.Y);
        return _drawable;
    }
    
    public Drawable Resize(Vector2 size)
    {
        Width = size.X;
        Height = size.Y;
        return _drawable;
    }

    public Drawable Translate(Vector2 coords)
    {
        Position = new ScalableVector2(Position.X.Value + coords.X, Position.Y.Value + coords.Y, Position.X.Scale,
            Position.Y.Scale);
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

    public Drawable Rotate(float radian)
    {
        Rotation += radian;
        return _drawable;
    }

    public void AddBorder(Color color, float thickness)
    {
        _drawable.AddBorder(color, thickness);
    }

    public ScalableVector2 Position
    {
        get => _drawable.Position;
        set => _drawable.Position = value;
    }

    public float X
    {
        get => _drawable.X;
        set => _drawable.X = value;
    }

    public float Y
    {
        get => _drawable.Y;
        set => _drawable.Y = value;
    }

    public ScalableVector2 Size
    {
        get => _drawable.Size;
        set => _drawable.Size = value;
    }

    public float Width
    {
        get => _drawable.Width;
        set => _drawable.Width = value;
    }

    public float Height
    {
        get => _drawable.Height;
        set => _drawable.Height = value;
    }

    public XnaVector2 Scale
    {
        get => _drawable.Scale;
        set => _drawable.Scale = value;
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
    
    public float Rotation
    {
        get => _drawable.Rotation;
        set => _drawable.Rotation = value;
    }

    public XnaVector2 Pivot
    {
        get => _drawable.Pivot;
        set => _drawable.Pivot = value;
    }

    public List<Drawable> Children => _drawable.Children;
}