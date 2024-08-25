using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using Vector = Quaver.Shared.Screens.Gameplay.ModCharting.Objects.ModChartVector;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyVector : ModChartProperty<Vector>
{
    public ModChartPropertyVector(Func<Vector> getter, Action<Vector> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyVector(Func<Vector> getter) : base(getter)
    {
    }

    public override Vector Add(Vector left, Vector right) => left + right;

    public override float Dot(Vector left, Vector right) => (float)Vector.Dot(left, right);

    public override Vector Normalise(Vector left) => left.Normalise();

    public override Vector Negative(Vector left) => -left;

    public override Vector Multiply(Vector left, float right) => left * right;

    public override Vector RandomUnit()
    {
        return Vector.RandomUnit(Getter()?.Dimension ?? 2);
    }

    public override LerpDelegate<Vector> Lerp => Vector.Lerp;
}