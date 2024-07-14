using System;
using System.Numerics;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyVector4 : ModChartProperty<Vector4>
{
    public ModChartPropertyVector4(Func<Vector4> getter, Action<Vector4> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyVector4(Func<Vector4> getter) : base(getter)
    {
    }

    public override Vector4 Add(Vector4 left, Vector4 right) => left + right;

    public override float Dot(Vector4 left, Vector4 right) => Vector4.Dot(left, right);

    public override Vector4 Normalise(Vector4 left) => Vector4.Normalize(left);

    public override Vector4 Negative(Vector4 left) => -left;

    public override Vector4 Multiply(Vector4 left, float right) => left * right;

    public override Vector4 RandomUnit()
    {
        var x = RandomHelper.RandomGauss();
        var y = RandomHelper.RandomGauss();
        var z = RandomHelper.RandomGauss();
        var w = RandomHelper.RandomGauss();
        var magnitude = MathF.Sqrt(x * x + y * y + z * z + w * w);
        return new Vector4(x / magnitude, y / magnitude, z / magnitude, w / magnitude);
    }

    public override LerpDelegate<Vector4> Lerp => Vector4.Lerp;
}