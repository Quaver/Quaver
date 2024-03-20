using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Tween;

public class TweenPayload : ISegmentPayload
{
    public delegate void SetterDelegate(float value);
    public delegate float EasingDelegate(float startValue, float endValue, float progress);
    public float StartValue { get; set; }
    public float EndValue { get; set; }
    public SetterDelegate Setter { get; set; }
    public EasingDelegate EasingFunction { get; set; }
    public void Update(float curTime, float progress)
    {
        if (progress is < 0 or > 1) return;
        Setter(EasingFunction(StartValue, EndValue, progress));
    }
}