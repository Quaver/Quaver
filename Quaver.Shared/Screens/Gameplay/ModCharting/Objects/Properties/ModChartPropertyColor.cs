using System;
using System.Numerics;
using Microsoft.Xna.Framework;
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

    protected override SetterDelegate<Color> SetterDelegate => TweenSetters.CreateColor(Setter);
}