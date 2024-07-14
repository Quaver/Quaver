using System;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
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

    public override Color Add(Color left, Color right)
    {
        return new Color(left.PackedValue + right.PackedValue);
    }

    public override Color Multiply(Color left, float right) => left * right;

    public override Color RandomUnit()
    {
        var x = RandomHelper.RandomGauss();
        var y = RandomHelper.RandomGauss();
        var z = RandomHelper.RandomGauss();
        var w = RandomHelper.RandomGauss();
        var magnitude = MathF.Sqrt(x * x + y * y + z * z + w * w);
        return new Color(x / magnitude, y / magnitude, z / magnitude, w / magnitude);
    }

    public TweenRainbowPayload TweenRainbow(float saturation, float lightness, int cycles = 1,
        EasingDelegate easingDelegate = default) => new(this, cycles, saturation, lightness)
    {
        EasingFunction = easingDelegate ?? EasingWrapperFunctions.Linear
    };

    public TweenRainbowPayload TweenRainbow(int cycles = 1, EasingDelegate easingDelegate = default) =>
        TweenRainbow(1, 0.5f, cycles, easingDelegate ?? EasingWrapperFunctions.Linear);


    public override LerpDelegate<Color> Lerp => Color.Lerp;
}