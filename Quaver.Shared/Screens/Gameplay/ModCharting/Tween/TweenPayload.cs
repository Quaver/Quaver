using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

[MoonSharpUserData]
public class TweenPayload<T> : ISegmentPayload
{
    public T StartValue { get; init; }
    public T EndValue { get; init; }
    public SetterDelegate<T> Setter { get; init; }
    public EasingDelegate EasingFunction { get; init; } = EasingWrapperFunctions.Linear;
    public void Update(float progress, Segment segment)
    {
        if (progress is < 0 or > 1) return;
        Setter(StartValue, EndValue, EasingFunction(progress));
    }

    public override string ToString()
    {
        return $"Tween[{StartValue} - {EndValue}]";
    }
}

public delegate void SetterDelegate<in T>(T startValue, T endValue, float progress);

public delegate float EasingDelegate(float progress);