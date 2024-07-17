using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Presets;

public class SwapLanePreset : ModChartPreset
{
    public int Lane1 { get; }

    public int Lane2 { get; }

    public SwapLanePreset(ElementAccessShortcut shortcut, int lane1, int lane2) : base(shortcut)
    {
        Lane1 = lane1;
        Lane2 = lane2;
    }

    protected override void PlacePreset(int startTime, int endTime)
    {
        var laneContainer1 = new DrawableProxy(Stage.LaneContainer(Lane1));
        var laneContainer2 = new DrawableProxy(Stage.LaneContainer(Lane2));
        Timeline.Add(startTime, endTime,
            laneContainer1.XProp.TweenSwap(laneContainer2.XProp, EasingDelegate));
        Timeline.Add(startTime, endTime,
            laneContainer1.YProp.TweenSwap(laneContainer2.YProp, EasingDelegate));
        Timeline.Add(startTime, endTime,
            laneContainer1.RotationProp.TweenSwap(laneContainer2.RotationProp, EasingDelegate));
        Timeline.Add(startTime, endTime,
            laneContainer1.ScaleProp.TweenSwap(laneContainer2.ScaleProp, EasingDelegate));
    }
}