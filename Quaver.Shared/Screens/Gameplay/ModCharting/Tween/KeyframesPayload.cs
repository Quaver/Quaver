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

    public readonly float StartTime;

    public readonly float EndTime;

    public readonly float Length;


    public KeyframesPayload(Keyframe<T>[] keyframes, SetterDelegate<T> setter)
    {
        Keyframes = keyframes;
        Array.Sort(Keyframes, Keyframe<T>.TimeComparer);
        StartTime = Keyframes.Length == 0 ? -1 : Keyframes[0].Time;
        EndTime = Keyframes.Length == 0 ? -1 : Keyframes[^1].Time;
        Length = EndTime - StartTime;
        Setter = setter;
    }

    public void Update(float progress, Segment segment)
    {
        if (Keyframes.Length < 1)
            return;

        var time = progress / Length + StartTime;
        while (_currentKeyframeIndex < Keyframes.Length - 1 && Keyframes[_currentKeyframeIndex + 1].Time < time)
            _currentKeyframeIndex++;

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