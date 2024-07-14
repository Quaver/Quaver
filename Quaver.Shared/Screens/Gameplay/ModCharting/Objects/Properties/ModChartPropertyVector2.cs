using System;
using System.Numerics;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyVector2 : ModChartProperty<Vector2>
{
    public ModChartPropertyVector2(Func<Vector2> getter, Action<Vector2> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyVector2(Func<Vector2> getter) : base(getter)
    {
    }

    public override Vector2 Add(Vector2 left, Vector2 right)
    {
        return left + right;
    }

    public override Vector2 Multiply(Vector2 left, float right) => left * right;

    public override Vector2 RandomUnit()
    {
        var x = RandomHelper.RandomGauss();
        var y = RandomHelper.RandomGauss();
        var magnitude = MathF.Sqrt(x * x + y * y);
        return new Vector2(x / magnitude, y / magnitude);
    }

    public override LerpDelegate<Vector2> Lerp => Vector2.Lerp;
}