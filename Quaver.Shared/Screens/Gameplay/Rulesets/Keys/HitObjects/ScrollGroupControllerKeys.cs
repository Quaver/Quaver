using System;
using System.Collections.Generic;
using System.Diagnostics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Wobble.Graphics.Animations;
using Wobble.Window;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

/// <summary>
///     This group is controlled by its own <see cref="ScrollGroup"/> which has its own list of SVs.
///     It spawns <see cref="ScrollNoteController"/>s
/// </summary>
/// <seealso cref="ScrollNoteController"/>
public class ScrollGroupControllerKeys : TimingGroupControllerKeys
{
    public ScrollGroupControllerKeys(TimingGroup timingGroup, Qua map, HitObjectManagerKeys manager) :
        base(timingGroup, map, manager)
    {
        Debug.Assert(timingGroup is ScrollGroup);
        // Initialize SV
        Populate();
        InitializePositionMarkers();
    }

    public override float ScrollSpeed => base.ScrollSpeed * CurrentScrollSpeedFactor;

    public ScrollGroup ScrollGroup => (ScrollGroup)TimingGroup;

    /// <summary>
    ///     Current SV index used for optimization when using UpdateCurrentPosition()
    ///     Default value is 0. "0" means that Current time has not passed first SV point yet.
    /// </summary>
    private int CurrentSvIndex { get; set; } = 0;

    private int CurrentSfIndex { get; set; } = -1;

    private List<SliderVelocityInfo> ScrollVelocityInfos { get; set; } = new();

    private List<ScrollSpeedFactorInfo> ScrollSpeedFactorInfos { get; set; } = new();

    public float CurrentScrollSpeedFactor { get; set; } = 1;

    /// <summary>
    ///     List of added hit object positions calculated from SV. Used for optimization
    /// </summary>
    public List<long> VelocityPositionMarkers { get; set; } = new List<long>();

    /// <summary>
    ///     Loose upper bound of the number of hitobjects on screen at one time.
    /// </summary>
    public int MaxHitObjectCount { get; private set; }

    /// <summary>
    ///     Only objects within this distance of the <see cref="CurrentTrackPosition"/> are rendered.
    /// </summary>
    public override long RenderThreshold =>
        (long)Math.Abs(WindowManager.Height * HitObjectManagerKeys.TrackRounding / ScrollSpeed);

    /// <summary>
    ///     Create SV-position points for computation optimization
    /// </summary>
    private void InitializePositionMarkers()
    {
        if (ScrollVelocityInfos.Count == 0)
            return;

        // Compute for Change Points
        var position = (long)(ScrollVelocityInfos[0].StartTime * ScrollGroup.InitialScrollVelocity *
                              HitObjectManagerKeys.TrackRounding);
        VelocityPositionMarkers.Add(position);

        for (var i = 1; i < ScrollVelocityInfos.Count; i++)
        {
            var multiplier = ScrollVelocityInfos[i - 1].Multiplier;
            if (float.IsNaN(multiplier))
                multiplier = 0;
            position += (long)((ScrollVelocityInfos[i].StartTime - ScrollVelocityInfos[i - 1].StartTime)
                               * multiplier * HitObjectManagerKeys.TrackRounding);
            VelocityPositionMarkers.Add(position);
        }
    }

    /// <summary>
    ///     Generates the actual list of scroll velocities used.
    ///     This combines the SVs in its own group + the SVs from the global SV group.
    /// </summary>
    private void Populate()
    {
        ScrollVelocityInfos = new List<SliderVelocityInfo>(ScrollGroup.ScrollVelocities);
        ScrollSpeedFactorInfos = new List<ScrollSpeedFactorInfo>(ScrollGroup.ScrollSpeedFactors);
        if (!ReferenceEquals(ScrollGroup, Manager.Ruleset.Map.GlobalScrollGroup))
        {
            ScrollVelocityInfos.InsertSorted(Manager.Ruleset.Map.GlobalScrollGroup.ScrollVelocities);
            ScrollSpeedFactorInfos.InsertSorted(Manager.Ruleset.Map.GlobalScrollGroup.ScrollSpeedFactors);
        }
    }

    /// <summary>
    ///     Get Hit Object (End/Start) position from audio time
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public override long GetPositionFromTime(double time)
    {
        var i = Math.Clamp(ScrollVelocityInfos.IndexAtTime((float)time) + 1, 0, ScrollVelocityInfos.Count);
        return GetPositionFromTime(time, i);
    }

    /// <summary>
    ///     Get Hit Object (End/Start) position from audio time and SV Index.
    ///     Index used for optimization
    /// </summary>
    /// <param name="time"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public long GetPositionFromTime(double time, int index)
    {
        // NoSV Modifier is toggled on
        if (Ruleset.ScoreProcessor.Mods.HasFlag(ModIdentifier.NoSliderVelocity))
            return (long)(time * HitObjectManagerKeys.TrackRounding);

        if (index == 0)
        {
            // Time starts before the first SV point
            return (long)(time * ScrollGroup.InitialScrollVelocity * HitObjectManagerKeys.TrackRounding);
        }

        index--;

        var curPos = VelocityPositionMarkers[index];
        var multiplier = ScrollVelocityInfos[index].Multiplier;
        if (float.IsNaN(multiplier))
            multiplier = 0;
        curPos += (long)((time - ScrollVelocityInfos[index].StartTime) *
                         multiplier *
                         HitObjectManagerKeys.TrackRounding);
        return curPos;
    }

    /// <summary>
    ///     Returns the scroll speed factor at that time on <see cref="sfIndex"/>.
    ///     The factor is lerped with the next factor, unless it is already the last.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="sfIndex"></param>
    /// <returns></returns>
    private float GetScrollSpeedFactorFromTime(double time, int sfIndex)
    {
        sfIndex = Math.Min(sfIndex, ScrollSpeedFactorInfos.Count - 1);
        if (sfIndex < 0 || Ruleset.ScoreProcessor.Mods.HasFlag(ModIdentifier.NoSliderVelocity))
            return 1;
        var sf = ScrollSpeedFactorInfos[sfIndex];
        if (sfIndex == ScrollSpeedFactorInfos.Count - 1)
            return sf.Multiplier;
        var nextSf = ScrollSpeedFactorInfos[sfIndex + 1];
        return EasingFunctions.Linear(sf.Multiplier, nextSf.Multiplier,
            ((float)time - sf.StartTime) / (nextSf.StartTime - sf.StartTime));
    }

    /// <summary>
    ///     Get SV direction changes between startTime and endTime.
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public List<SVDirectionChange> GetSVDirectionChanges(double startTime, double endTime)
    {
        var changes = new List<SVDirectionChange>();

        if (Ruleset.ScoreProcessor.Mods.HasFlag(ModIdentifier.NoSliderVelocity))
            return changes;

        // Find the first SV index.
        int i;
        for (i = 0; i < ScrollVelocityInfos.Count; i++)
        {
            if (startTime < ScrollVelocityInfos[i].StartTime)
                break;
        }

        bool forward;
        if (i == 0)
            forward = ScrollGroup.InitialScrollVelocity >= 0;
        else
            forward = ScrollVelocityInfos[i - 1].Multiplier >= 0;

        // Loop over SV changes between startTime and endTime.
        for (; i < ScrollVelocityInfos.Count && endTime >= ScrollVelocityInfos[i].StartTime; i++)
        {
            var multiplier = ScrollVelocityInfos[i].Multiplier;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (multiplier == 0)
                // Zero speed means we're staying in the same spot.
                continue;

            if (forward == (multiplier > 0))
                // The direction hasn't changed.
                continue;

            forward = multiplier > 0;
            changes.Add(new SVDirectionChange
            {
                StartTime = ScrollVelocityInfos[i].StartTime, Position = VelocityPositionMarkers[i]
            });
        }

        return changes;
    }

    /// <summary>
    ///     Returns true if the playfield is going backwards at the given time.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool IsSVNegative(double time)
    {
        if (Ruleset.ScoreProcessor.Mods.HasFlag(ModIdentifier.NoSliderVelocity))
            return false;

        // Find the SV index at time.
        int i;
        for (i = 0; i < ScrollVelocityInfos.Count; i++)
        {
            if (time < ScrollVelocityInfos[i].StartTime)
                break;
        }

        i--;

        // Find index of the last non-zero SV.
        for (; i >= 0; i--)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (ScrollVelocityInfos[i].Multiplier != 0)
                break;
        }

        if (i == -1)
            return ScrollGroup.InitialScrollVelocity < 0;

        return ScrollVelocityInfos[i].Multiplier < 0;
    }

    public override void HandleSkip()
    {
        CurrentSvIndex = 0;
        CurrentSfIndex = -1;
    }

    /// <summary>
    ///     Update Current position of the hit objects
    /// </summary>
    /// <param name="audioTime"></param>
    public override sealed void UpdateCurrentTrackPosition()
    {
        // Update SV index if necessary. Afterwards update Position.
        while (CurrentSvIndex < ScrollVelocityInfos.Count &&
               Manager.CurrentVisualAudioOffset >= ScrollVelocityInfos[CurrentSvIndex].StartTime)
        {
            CurrentSvIndex++;
        }
        
        while (CurrentSfIndex < ScrollSpeedFactorInfos.Count - 1 && Manager.CurrentVisualAudioOffset >= ScrollSpeedFactorInfos[CurrentSfIndex + 1].StartTime)
        {
            CurrentSfIndex++;
        }

        CurrentScrollSpeedFactor = GetScrollSpeedFactorFromTime(Manager.CurrentVisualAudioOffset, CurrentSfIndex);


        CurrentTrackPosition = GetPositionFromTime(Manager.CurrentVisualAudioOffset, CurrentSvIndex);
        if (CurrentTrackPosition < -9223372036852000000)
        {
            ;
            GetPositionFromTime(Manager.CurrentVisualAudioOffset, CurrentSvIndex);
        }
    }

    /// <summary>
    ///     Creates a <see cref="ScrollNoteController"/> that controls <see cref="hitObject"/> from <see cref="ScrollGroup"/>
    /// </summary>
    /// <param name="hitObject"></param>
    /// <param name="render"></param>
    /// <returns></returns>
    public override NoteControllerKeys CreateNoteController(HitObjectInfo hitObject, bool render)
    {
        var scrollNoteController = new ScrollNoteController(hitObject, this);
        if (render)
            NoteControllersToRender.Add(scrollNoteController);
        return scrollNoteController;
    }
}