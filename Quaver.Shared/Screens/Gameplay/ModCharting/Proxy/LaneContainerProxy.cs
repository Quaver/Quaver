using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class LaneContainerProxy : ContainerProxy
{
    private readonly GameplayPlayfieldLane _lane;
    public LaneContainerProxy(GameplayPlayfieldLane lane) : base(lane)
    {
        _lane = lane;
    }

    public Sprite Receptor => _lane.Receptor;

    public Container HitObjectContainer => _lane.HitObjectContainer;

    /// <summary>
    ///     1-indexed lane number
    /// </summary>
    public int Lane => _lane.Lane + 1;

    public float LaneSize => _lane.LaneSize;

    public float LaneScale => _lane.LaneScale;

    public ColumnLighting ColumnLighting => _lane.ColumnLighting;
}