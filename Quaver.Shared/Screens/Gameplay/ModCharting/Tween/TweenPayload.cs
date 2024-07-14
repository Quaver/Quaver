using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

[MoonSharpUserData]
public class TweenPayload<T> : PropertySegmentPayload<T>
{
    public T StartValue { get; set; }
    public T EndValue { get; set; }

    public TweenPayload(ModChartProperty<T> modChartProperty)
    {
        Property = modChartProperty;
    }

    public EasingDelegate EasingFunction { get; init; } = EasingWrapperFunctions.Linear;

    public override void Update(float progress, Segment segment)
    {
        if (progress is < 0 or > 1) return;
        progress = EasingFunction(progress);
        var lerpedValue = Property.Lerp(StartValue, EndValue, progress);
        Property.Value = progress is 0 or 1 ? lerpedValue : Transform(lerpedValue, progress);
    }

    public override string ToString()
    {
        return $"Tween[{StartValue} - {EndValue}]";
    }

    public virtual void OnEnter(Segment segment)
    {
    }

    public virtual void OnLeave(Segment segment)
    {
    }
}

public delegate T LerpDelegate<T>(T startValue, T endValue, float progress);

public delegate float EasingDelegate(float progress);