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
    }
}