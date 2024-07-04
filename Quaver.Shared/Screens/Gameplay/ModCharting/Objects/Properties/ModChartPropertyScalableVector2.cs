using System;
using System.Numerics;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyScalableVector2 : ModChartProperty<ScalableVector2>
{
    public ModChartPropertyScalableVector2(Func<ScalableVector2> getter, Action<ScalableVector2> setter) : base(getter,
        setter)
    {
    }

    public ModChartPropertyScalableVector2(Func<ScalableVector2> getter) : base(getter)
    {
    }

    protected override ScalableVector2 Add(ScalableVector2 left, ScalableVector2 right)
    {
        return new ScalableVector2(left.X.Value + right.X.Value, left.Y.Value + right.Y.Value,
            left.X.Scale + right.X.Scale, left.Y.Scale + right.Y.Scale);
    }

    protected override SetterDelegate<ScalableVector2> SetterDelegate => TweenSetters.CreateScalableVector2(Setter);
}