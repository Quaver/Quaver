using MoonSharp.Interpreter;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

public class ContainerProxy : DrawableProxy
{
    private Container _drawable;

    [MoonSharpHidden]
    public ContainerProxy(Container drawable) : base(drawable)
    {
        _drawable = drawable;
    }
}