using System;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class TweenPayload : ISegmentPayload
{
    public delegate void SetterDelegate(float value);
    public float StartValue { get; set; }
    public float EndValue { get; set; }
    public SetterDelegate Setter { get; set; }
    public Easing Easing { get; set; } = Easing.Linear;
    public void Update(float curTime, float progress)
    {
        if (progress is < 0 or > 1) return;
        Setter(EasingFunctions.Perform(Easing, StartValue, EndValue, progress));
    }
}