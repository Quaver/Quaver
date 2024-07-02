using System;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

[MoonSharpUserData]
public class CustomTriggerPayload: ITriggerPayload
{
    public Action<ValueVertex<ITriggerPayload>> TriggerClosure { get; set; }

    public CustomTriggerPayload(Action<ValueVertex<ITriggerPayload>> triggerClosure)
    {
        TriggerClosure = triggerClosure;
    }

    public void Trigger(ValueVertex<ITriggerPayload> valueVertex)
    {
        TriggerClosure?.Invoke(valueVertex);
    }

    public override string ToString()
    {
        return $"Custom";
    }
}