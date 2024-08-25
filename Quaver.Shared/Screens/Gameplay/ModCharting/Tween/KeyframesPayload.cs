using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

[MoonSharpUserData]
public class KeyframesPayload<T> : PropertySegmentPayload<T>
{
    public Keyframe<T>[] Keyframes { get; }

    private int _currentKeyframeIndex;

    public readonly double RelativeLength;


    public KeyframesPayload(ModChartProperty<T> property, Keyframe<T>[] keyframes)
    {
        Keyframes = keyframes;
        Property = property;
        Array.Sort(Keyframes, Keyframe<T>.TimeComparer);
        if (Keyframes.Length < 2 || Keyframes[0].Time != 0)
        {
            throw new ScriptRuntimeException(
                "Keyframes payload must have at least two keyframes and must start at time 0!");
        }

        RelativeLength = Keyframes[^1].Time;
    }

    public override void Update(float progress, Segment segment)
    {
        if (Keyframes.Length < 1)
            return;

        var time = progress * RelativeLength;

        while (_currentKeyframeIndex < Keyframes.Length - 1 && Keyframes[_currentKeyframeIndex + 1].Time < time)
            _currentKeyframeIndex++;

        while (_currentKeyframeIndex > 0 && Keyframes[_currentKeyframeIndex].Time > time)
            _currentKeyframeIndex--;

        var currentKeyFrame = Keyframes[_currentKeyframeIndex];
        T lerpedValue;
        if (_currentKeyframeIndex == Keyframes.Length - 1)
        {
            lerpedValue = Property.Lerp(currentKeyFrame.Value, currentKeyFrame.Value, 1);
        }
        else
        {
            var nextKeyframe = Keyframes[_currentKeyframeIndex + 1];
            lerpedValue = Lerp(currentKeyFrame.Value, nextKeyframe.Value,
                currentKeyFrame.GetProgress(nextKeyframe, time));
        }

        Property.Value = progress is 0 or 1 ? lerpedValue : Transform(lerpedValue, progress);
    }
}