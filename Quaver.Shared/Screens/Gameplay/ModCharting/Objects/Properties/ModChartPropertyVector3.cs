using System;
using System.Numerics;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyVector3 : ModChartProperty<Vector3>
{
    public ModChartPropertyVector3(Func<Vector3> getter, Action<Vector3> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyVector3(Func<Vector3> getter) : base(getter)
    {
    }

    public override Vector3 Add(Vector3 left, Vector3 right)
    {
        return left + right;
    }

    public override Vector3 Multiply(Vector3 left, float right) => left * right;

    public override Vector3 RandomUnit()
    {
        var x = RandomHelper.RandomGauss();
        var y = RandomHelper.RandomGauss();
        var z = RandomHelper.RandomGauss();
        var magnitude = MathF.Sqrt(x * x + y * y + z * z);
        return new Vector3(x / magnitude, y / magnitude, z / magnitude);
    }

    public override LerpDelegate<Vector3> Lerp => Vector3.Lerp;
}