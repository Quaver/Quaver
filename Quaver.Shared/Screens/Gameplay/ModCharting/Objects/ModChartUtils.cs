using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartUtils : ModChartGlobalVariable
{
    public ModChartUtils(ElementAccessShortcut shortcut) : base(shortcut)
    {
    }

    private readonly List<int> _timingPointMeasures = new();

    [MoonSharpHidden]
    public void InitializeMeasures()
    {
        _timingPointMeasures.Clear();
        var timingPointInfos = Shortcut.GameplayScreenView.Screen.Map.TimingPoints;
        if (timingPointInfos.Count < 1)
            return;
        _timingPointMeasures.Add(0);
        var currentTimingPoint = timingPointInfos[0];
        for (var index = 0; index < timingPointInfos.Count - 1; index++)
        {
            var nextTimingPoint = timingPointInfos[index + 1];
            var beatsBetween = (nextTimingPoint.StartTime - currentTimingPoint.StartTime) /
                               currentTimingPoint.MillisecondsPerBeat;
            var measuresBetween = (int)MathF.Ceiling(beatsBetween / (int)currentTimingPoint.Signature);
            _timingPointMeasures.Add(_timingPointMeasures[^1] + measuresBetween);
            currentTimingPoint = nextTimingPoint;
        }
    }

    /// <summary>
    ///     Gets the time at the specified measure and beat
    /// </summary>
    /// <param name="measure">Bar number. Should be same as shown in the editor.</param>
    /// <param name="beat">Number of beats (fraction of measure) starting at 0. Can be a float.</param>
    /// <returns></returns>
    public float Beat(int measure, float beat)
    {
        measure--;
        var timingPointIndex = _timingPointMeasures.BinarySearch(measure);
        if (timingPointIndex < 0)
            timingPointIndex = ~timingPointIndex - 1;
        if (timingPointIndex < 0) // == -1
            return int.MinValue;

        var timingPoint = Shortcut.GameplayScreenView.Screen.Map.TimingPoints[timingPointIndex];
        var startingMeasure = _timingPointMeasures[timingPointIndex];
        var totalBeats = (measure - startingMeasure) * (int)timingPoint.Signature + beat;
        return timingPoint.StartTime + totalBeats * timingPoint.MillisecondsPerBeat;
    }

    /// <summary>
    ///     Gets the time at the specified decimal measure.
    /// </summary>
    /// <param name="measure">Bar number + fraction of measure</param>
    /// <returns></returns>
    public float Measure(float measure)
    {
        measure--;
        var timingPointIndex = _timingPointMeasures.BinarySearch((int)measure);
        if (timingPointIndex < 0)
            timingPointIndex = ~timingPointIndex - 1;
        if (timingPointIndex < 0) // == -1
            return int.MinValue;

        var timingPoint = Shortcut.GameplayScreenView.Screen.Map.TimingPoints[timingPointIndex];
        var startingMeasure = _timingPointMeasures[timingPointIndex];
        var totalBeats = (measure - startingMeasure) * (int)timingPoint.Signature;
        return timingPoint.StartTime + totalBeats * timingPoint.MillisecondsPerBeat;
    }
}