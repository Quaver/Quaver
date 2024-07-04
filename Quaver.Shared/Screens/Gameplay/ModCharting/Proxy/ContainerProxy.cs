using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class ContainerProxy : DrawableProxy
{
    private readonly Container _drawable;

    [MoonSharpHidden]
    public ContainerProxy(Container drawable) : base(drawable)
    {
        _drawable = drawable;
        RenderTargetBackgroundColorProp = new ModChartPropertyColor(() => _drawable.RenderTargetOptions.BackgroundColor,
            v => _drawable.RenderTargetOptions.BackgroundColor = v);
    }

    public void CastToRenderTarget() => _drawable.CastToRenderTarget();

    public RenderProjectionSprite DefaultProjectionSprite => _drawable.DefaultProjectionSprite;

    public Padding OverflowRenderPadding
    {
        get => _drawable.RenderTargetOptions.OverflowRenderPadding;
        set => _drawable.RenderTargetOptions.OverflowRenderPadding = value;
    }

    public Color RenderTargetBackgroundColor
    {
        get => _drawable.RenderTargetOptions.BackgroundColor;
        set => _drawable.RenderTargetOptions.BackgroundColor = value;
    }

    public readonly ModChartPropertyColor RenderTargetBackgroundColorProp;
}