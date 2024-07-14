using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
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

    public override ScalableVector2 Add(ScalableVector2 left, ScalableVector2 right)
    {
        return new ScalableVector2(left.X.Value + right.X.Value, left.Y.Value + right.Y.Value,
            left.X.Scale + right.X.Scale, left.Y.Scale + right.Y.Scale);
    }

    public override ScalableVector2 Multiply(ScalableVector2 left, float right)
    {
        return new ScalableVector2(left.X.Value * right, left.Y.Value * right, left.X.Scale * right,
            left.Y.Scale * right);
    }

    public override ScalableVector2 RandomUnit()
    {
        var vec = RandomHelper.RandomUnitXnaVector2();
        return new ScalableVector2(vec.X, vec.Y);
    }

    public override LerpDelegate<ScalableVector2> Lerp => ScalableVector2.Lerp;
}