using System;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

[MoonSharpUserData]
public class CustomSegmentPayload : ISegmentPayload
{
    public delegate void SegmentUpdater(float progress, Segment segment);
    public SegmentUpdater Updater { get; }

    public CustomSegmentPayload(SegmentUpdater updater)
    {
        Updater = updater;
    }

    [MoonSharpHidden]
    public void Update(float progress, Segment segment)
    {
        Updater?.Invoke(progress, segment);
    }

    public override string ToString()
    {
        return "Custom";
    }
}