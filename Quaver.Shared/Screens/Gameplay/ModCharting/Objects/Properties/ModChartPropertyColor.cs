using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyColor : ModChartProperty<Color>
{
    public ModChartPropertyColor(Func<Color> getter, Action<Color> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyColor(Func<Color> getter) : base(getter)
    {
    }

    protected override Color Add(Color left, Color right)
    {
        return new Color(left.PackedValue + right.PackedValue);
    }

    public TweenPayload<float> TweenRainbow(float saturation, float lightness, int cycles = 1,
        EasingDelegate easingDelegate = default)
    {
        var originalColor = Getter();
        return new TweenPayload<float>
        {
            StartValue = 0,
            EndValue = cycles,
            EasingFunction = easingDelegate ?? EasingWrapperFunctions.Linear,
            Setter = (_, _, progress) =>
            {
                var color = progress is > 0 and < 1
                    ? ColorHelper.FromHsl(progress * cycles % 1f, saturation, lightness)
                    : originalColor;
                Setter(color);
            }
        };
    }

    public TweenPayload<float> TweenRainbow(int cycles = 1, EasingDelegate easingDelegate = default) => 
        TweenRainbow(1, 0.5f, cycles, easingDelegate ?? EasingWrapperFunctions.Linear);


    protected override SetterDelegate<Color> SetterDelegate => TweenSetters.CreateColor(Setter);
}