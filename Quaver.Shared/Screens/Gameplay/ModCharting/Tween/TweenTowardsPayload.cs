using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

/// <summary>
///     Tween that doesn't care about the starting value. Once entered it will tween from current value to specified end value.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TweenTowardsPayload<T> : TweenPayload<T>
{
    public TweenTowardsPayload(ModChartProperty<T> modChartProperty) : base(modChartProperty)
    {
    }

    public override void OnEnter(Segment segment)
    {
        base.OnEnter(segment);
        StartValue = Property.Value;
    }
}