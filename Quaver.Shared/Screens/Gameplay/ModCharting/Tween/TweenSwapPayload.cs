using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

/// <summary>
///     Tween that swaps two properties.
/// </summary>
/// <typeparam name="T"></typeparam>
[MoonSharpUserData]
public class TweenSwapPayload<T> : TweenPayload<T>
{
    /// <summary>
    ///     The other property to swap with <see cref="PropertySegmentPayload{T}.Property"/>
    /// </summary>
    private ModChartProperty<T> Property2 { get; init; }

    public override void Update(float progress, Segment segment)
    {
        if (progress is < 0 or > 1) return;
        progress = EasingFunction(progress);
        var lerpedValue1 = Property.Lerp(StartValue, EndValue, progress);
        var lerpedValue2 = Property2.Lerp(EndValue, StartValue, progress);
        if (progress is 0 or 1)
        {
            lerpedValue1 = Transform(lerpedValue1, progress);
            lerpedValue2 = Transform(lerpedValue2, progress);
        }

        Property.Value = lerpedValue1;
        Property2.Value = lerpedValue2;
    }

    public override void OnEnter(Segment segment)
    {
        base.OnEnter(segment);
        StartValue = Property.Value;
        EndValue = Property2.Value;
    }

    public override string ToString()
    {
        return $"TweenSwap[{StartValue} - {EndValue}]";
    }

    public TweenSwapPayload(ModChartProperty<T> modChartProperty, ModChartProperty<T> property2) : base(
        modChartProperty)
    {
        Property2 = property2;
    }
}