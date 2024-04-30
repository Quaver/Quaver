using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public class ModChartEventTriggerArgs : ModChartEventArgs
{
    public ValueVertex<ITriggerPayload> Vertex { get; }
    public bool Trigger { get; }

    public ModChartEventTriggerArgs(ValueVertex<ITriggerPayload> vertex, bool trigger = true)
    {
        Vertex = vertex;
        Trigger = trigger;
    }
}