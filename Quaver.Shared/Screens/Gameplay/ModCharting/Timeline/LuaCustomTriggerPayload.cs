using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

[MoonSharpUserData]
public class LuaCustomTriggerPayload: ITriggerPayload
{
    public Closure TriggerClosure { get; set; }

    public LuaCustomTriggerPayload(Closure triggerClosure)
    {
        TriggerClosure = triggerClosure;
    }

    public void Trigger(ValueVertex<ITriggerPayload> valueVertex)
    {
        TriggerClosure?.SafeCall(valueVertex);
    }

    public override string ToString()
    {
        return $"Custom";
    }
}