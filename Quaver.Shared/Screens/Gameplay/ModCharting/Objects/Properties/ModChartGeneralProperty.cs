using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartGeneralProperty<T>
{
    protected readonly Action<T> Setter;
    protected readonly Func<T> Getter;

    public ModChartGeneralProperty(Func<T> getter, Action<T> setter)
    {
        Getter = getter;
        Setter = setter;
    }

    public ModChartGeneralProperty(Func<T> getter)
    {
        Getter = getter;
        Setter = _ => { };
    }

    public T Value
    {
        get => Getter();
        set => Setter(value);
    }

    public ITriggerPayload TriggerSet(T value) => new CustomTriggerPayload(_ => Setter(value));

    public ITriggerPayload TriggerSwap(ModChartGeneralProperty<T> other) => new CustomTriggerPayload(_ =>
    {
        var tmp = Getter();
        Setter(other.Getter());
        other.Setter(tmp);
    });
}