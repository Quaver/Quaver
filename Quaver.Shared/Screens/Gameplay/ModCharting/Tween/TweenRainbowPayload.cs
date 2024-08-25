using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

/// <summary>
///     Tween that achieves the rainbow effect using HSL color space
/// </summary>
public class TweenRainbowPayload : TweenPayload<Color>
{
    private int Cycles { get; }
    private float Saturation { get; }
    private float Lightness { get; }

    public TweenRainbowPayload(ModChartProperty<Color> modChartProperty, int cycles, float saturation, float lightness)
        : base(modChartProperty)
    {
        Cycles = cycles;
        Saturation = saturation;
        Lightness = lightness;
    }

    public override void Update(float progress, Segment segment)
    {
        if (progress is < 0 or > 1) return;
        progress = EasingFunction(progress);
        var color = progress is > 0 and < 1
            ? ColorHelper.FromHsl(progress * Cycles % 1f, Saturation, Lightness)
            : StartValue;
        Property.Value = progress is 0 or 1 ? color : Transform(color, progress);
    }

    public override void OnEnter(Segment segment)
    {
        base.OnEnter(segment);
        StartValue = Property.Value;
    }
}