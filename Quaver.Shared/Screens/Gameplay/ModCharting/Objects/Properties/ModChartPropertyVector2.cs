using System;
using System.Numerics;
using MoonSharp.Interpreter;
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

    protected override Vector2 Add(Vector2 left, Vector2 right)
    {
        return left + right;
    }

    protected override SetterDelegate<Vector2> SetterDelegate => TweenSetters.CreateVector2(Setter);
}