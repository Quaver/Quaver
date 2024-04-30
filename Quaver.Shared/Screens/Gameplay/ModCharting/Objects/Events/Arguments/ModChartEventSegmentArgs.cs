using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public class ModChartEventSegmentArgs : ModChartEventArgs
{
    public Segment Segment { get; }

    public ModChartEventSegmentArgs(Segment segment)
    {
        Segment = segment;
    }
}