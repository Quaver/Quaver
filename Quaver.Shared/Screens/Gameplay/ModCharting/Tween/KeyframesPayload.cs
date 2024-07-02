using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

[MoonSharpUserData]
public class KeyframesPayload<T> : ISegmentPayload
{
    public Keyframe<T>[] Keyframes { get; }

    public SetterDelegate<T> Setter { get; }

    private int _currentKeyframeIndex;
    public readonly double RelativeLength;


    public KeyframesPayload(SetterDelegate<T> setter, Keyframe<T>[] keyframes)
    {
        Keyframes = keyframes;
        Array.Sort(Keyframes, Keyframe<T>.TimeComparer);
        if (Keyframes.Length < 2 || Keyframes[0].Time != 0)
        {
            throw new ScriptRuntimeException(
                "Keyframes payload must have at least two keyframes and must start at time 0!");
        }

        RelativeLength = Keyframes[^1].Time;
        Setter = setter;
    }

    public void Update(float progress, Segment segment)
    {
        if (Keyframes.Length < 1)
            return;

        var time = progress * RelativeLength;

        while (_currentKeyframeIndex < Keyframes.Length - 1 && Keyframes[_currentKeyframeIndex + 1].Time < time)
            _currentKeyframeIndex++;

        while (_currentKeyframeIndex > 0 && Keyframes[_currentKeyframeIndex].Time > time)
            _currentKeyframeIndex--;

        var currentKeyFrame = Keyframes[_currentKeyframeIndex];
        if (_currentKeyframeIndex == Keyframes.Length - 1)
        {
            Setter(currentKeyFrame.Value, currentKeyFrame.Value, 1);
        }
        else
        {
            var nextKeyframe = Keyframes[_currentKeyframeIndex + 1];
            Setter(currentKeyFrame.Value, nextKeyframe.Value, currentKeyFrame.GetProgress(nextKeyframe, time));
        }
    }
}