using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

public class ContainerProxy : DrawableProxy
{
    private Container _container;
    public ContainerProxy(Container container) : base(container)
    {
        _container = container;
    }
}