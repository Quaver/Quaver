using System;
using MoonSharp.Interpreter;
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

    protected override XnaVector2 Add(XnaVector2 left, XnaVector2 right)
    {
        return left + right;
    }

    protected override SetterDelegate<XnaVector2> SetterDelegate => TweenSetters.CreateXnaVector2(Setter);
}