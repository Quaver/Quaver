using System;
using MonoGame.Extended;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyXnaVector2 : ModChartProperty<XnaVector2>
{
    public ModChartPropertyXnaVector2(Func<XnaVector2> getter, Action<XnaVector2> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyXnaVector2(Func<XnaVector2> getter) : base(getter)
    {
    }

    public override XnaVector2 Add(XnaVector2 left, XnaVector2 right) => left + right;

    public override float Dot(XnaVector2 left, XnaVector2 right) => XnaVector2.Dot(left, right);

    public override XnaVector2 Normalise(XnaVector2 left) => left.NormalizedCopy();

    public override XnaVector2 Negative(XnaVector2 left) => -left;

    public override XnaVector2 Multiply(XnaVector2 left, float right) => left * right;

    public override XnaVector2 RandomUnit() => RandomHelper.RandomUnitXnaVector2();

    public override LerpDelegate<XnaVector2> Lerp => XnaVector2.Lerp;
}