using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Presets;

/// <summary>
///     Arranges the selected lanes next to each other, at a specified center and horizontal direction.
/// </summary>
[MoonSharpUserData]
public class LaneArrangePreset : ModChartPreset
{
    private readonly List<int> enabledLanes;
    private ScalableVector2 center;
    private Vector2 horizontalDirection = Vector2.UnitX;

    public LaneArrangePreset(ElementAccessShortcut shortcut, List<int> enabledLanes) : base(shortcut)
    {
        var keyCount = shortcut.GameplayScreen.Map.GetKeyCount();
        if (enabledLanes.FirstOrDefault(i => i > keyCount || i < 1, int.MinValue) is var invalidLane &&
            invalidLane != int.MinValue)
        {
            throw new ScriptRuntimeException(
                $"Lane {invalidLane} is not a valid lane! It should be between 1 and {keyCount} inclusively.");
        }

        this.enabledLanes = enabledLanes;
    }

    protected override void PlacePreset(int startTime, int endTime)
    {
        var centerFactor = enabledLanes.Count / 2f - 0.5f;
        var centerPos = new Vector2(center.X.Value, center.Y.Value);
        var laneContainers = Shortcut.GameplayPlayfieldKeysStage.LaneContainers;
        for (var i = 0; i < enabledLanes.Count; i++)
        {
            var lane = enabledLanes[i];
            var pos = centerPos + (i - centerFactor) * horizontalDirection * laneContainers[0].Width;
            var scalablePos = new ScalableVector2(pos.X, pos.Y, center.X.Scale, center.Y.Scale);
            var laneContainer = laneContainers[lane - 1];
            var laneContainerProxy = new LaneContainerProxy(laneContainer);
            Timeline.Add(startTime, endTime, laneContainerProxy.PositionProp.Tween(scalablePos, EasingDelegate));
            if (laneContainer.Scale == Vector2.UnitY)
                Timeline.Add(startTime, endTime, laneContainerProxy.ScaleProp.Tween(Vector2.One, EasingDelegate));
        }

        for (var i = 0; i < Shortcut.Internal.KeyCount; i++)
        {
            if (enabledLanes.Contains(i + 1))
                continue;
            var laneContainerProxy = new LaneContainerProxy(laneContainers[i]);
            Timeline.Add(startTime, endTime, laneContainerProxy.ScaleProp.Tween(Vector2.UnitY, EasingDelegate));
        }
    }

    public LaneArrangePreset Center(ScalableVector2 position)
    {
        center = position;
        return this;
    }

    public LaneArrangePreset Horizontal(Vector2 direction)
    {
        horizontalDirection = direction;
        return this;
    }
}