using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

/// <summary>
///     Does nothing. It is to be used when one only wishes to apply a modifer to some property.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TweenIdlePayload<T> : TweenPayload<T>
{
    public TweenIdlePayload(ModChartProperty<T> modChartProperty) : base(modChartProperty)
    {
    }

    public override void OnEnter(Segment segment)
    {
        base.OnEnter(segment);
        StartValue = EndValue = Property.Value;
    }
}