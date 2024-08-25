using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class SpriteProxy : DrawableProxy
{
    private readonly Sprite _drawable;

    [MoonSharpHidden]
    public SpriteProxy(Sprite drawable) : base(drawable)
    {
        _drawable = drawable;

        AlphaProp = new ModChartPropertyFloat(() => _drawable.Alpha, v => _drawable.Alpha = v);
        TintProp = new ModChartPropertyColor(() => _drawable.Tint, v => _drawable.Tint = v);
        SpriteRotationProp =
            new ModChartPropertyFloat(() => _drawable.SpriteRotation, v => _drawable.SpriteRotation = v);
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

    public SpriteBatchOptions SpriteBatchOptions
    {
        get => _drawable.SpriteBatchOptions;
        set
        {
            _drawable.SpriteBatchOptions = value;
            if (_drawable is RenderProjectionSprite renderProjectionSprite)
                renderProjectionSprite.UpdateShaderSizeParameter();
        }
    }

    public readonly ModChartPropertyFloat AlphaProp;

    public readonly ModChartPropertyColor TintProp;

    public bool IndependentRotation
    {
        get => _drawable.IndependentRotation;
        set => _drawable.IndependentRotation = value;
    }

    public float SpriteRotation
    {
        get => _drawable.SpriteRotation;
        set => _drawable.SpriteRotation = value;
    }

    public readonly ModChartPropertyFloat SpriteRotationProp;
}