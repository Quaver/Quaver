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
    public float BeatToTime(int measure, float beat)
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
    public float MeasureToTime(float measure)
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

    public DynValue Beat(ScriptExecutionContext ctx, CallbackArguments args)
    {
        if (args.Count == 0) throw new ScriptRuntimeException($"No arguments given to beat!");
        var measure = args[0].Number;
        if (args.Count == 1)
            return DynValue.NewNumber(MeasureToTime((float)measure));
        var beat = args[1].Number;
        if (args.Count > 4)
            throw new ScriptRuntimeException($"beat() accepts 1-4 parameters but {args.Count} are given!");
        if (args.Count == 3)
            beat += args[2].Number;
        else if (args.Count == 4)
            beat += args[2].Number / args[3].Number;

        return DynValue.NewNumber(BeatToTime((int)measure, (float)beat));
    }
}