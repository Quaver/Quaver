using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

/// <summary>
///     Increases the value of the property when the payload is active
/// </summary>
/// <typeparam name="T"></typeparam>
public class TweenAddPayload<T> : TweenPayload<T>
{
    private T increment;

    public TweenAddPayload(ModChartProperty<T> modChartProperty, T increment) : base(modChartProperty)
    {
        this.increment = increment;
    }

    public override void OnEnter(Segment segment)
    {
        base.OnEnter(segment);
        StartValue = Property.Value;
        EndValue = Property.Add(StartValue, increment);
    }
}