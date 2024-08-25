using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class LayerProxy
{
    private readonly Layer _layer;

    public LayerProxy(Layer layer)
    {
        _layer = layer;
    }

    public string Name => _layer.Name;

    public bool RequireBelow(Layer upperLayer) => _layer.RequireBelow(upperLayer);
    public bool StopRequireBelow(Layer upperLayer) => _layer.StopRequireBelow(upperLayer);

    public bool RequireAbove(Layer lowerLayer) => _layer.RequireAbove(lowerLayer);
    public bool StopRequireAbove(Layer lowerLayer) => _layer.StopRequireAbove(lowerLayer);

    public bool RequireBetween(Layer lowerLayer, Layer upperLayer) => _layer.RequireBetween(lowerLayer, upperLayer);
}