using System.Collections.Generic;
using System.Diagnostics;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
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
        InitializePositionMarkers();
        UpdateCurrentTrackPosition();
    }

    public ScrollGroup ScrollGroup => (ScrollGroup)TimingGroup;

    /// <summary>
    ///     Current SV index used for optimization when using UpdateCurrentPosition()
    ///     Default value is 0. "0" means that Current time has not passed first SV point yet.
    /// </summary>
    private int CurrentSvIndex { get; set; } = 0;

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
        (long)(WindowManager.Height * HitObjectManagerKeys.TrackRounding / ScrollSpeed);

    /// <summary>
    ///     Create SV-position points for computation optimization
    /// </summary>
    private void InitializePositionMarkers()
    {
        if (ScrollGroup.ScrollVelocities.Count == 0)
            return;

        // Compute for Change Points
        var position = (long)(ScrollGroup.ScrollVelocities[0].StartTime * ScrollGroup.InitialScrollVelocity *
                              HitObjectManagerKeys.TrackRounding);
        VelocityPositionMarkers.Add(position);

        for (var i = 1; i < ScrollGroup.ScrollVelocities.Count; i++)
        {
            position += (long)((ScrollGroup.ScrollVelocities[i].StartTime -
                                ScrollGroup.ScrollVelocities[i - 1].StartTime)
                               * ScrollGroup.ScrollVelocities[i - 1].Multiplier * HitObjectManagerKeys.TrackRounding);
            VelocityPositionMarkers.Add(position);
        }
    }

    /// <summary>
    ///     Get Hit Object (End/Start) position from audio time (Unoptimized.)
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public override long GetPositionFromTime(double time)
    {
        int i;
        for (i = 0; i < ScrollGroup.ScrollVelocities.Count; i++)
        {
            if (time < ScrollGroup.ScrollVelocities[i].StartTime)
                break;
        }

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
        curPos += (long)((time - ScrollGroup.ScrollVelocities[index].StartTime) *
                         ScrollGroup.ScrollVelocities[index].Multiplier *
                         HitObjectManagerKeys.TrackRounding);
        return curPos;
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
        for (i = 0; i < ScrollGroup.ScrollVelocities.Count; i++)
        {
            if (startTime < ScrollGroup.ScrollVelocities[i].StartTime)
                break;
        }

        bool forward;
        if (i == 0)
            forward = ScrollGroup.InitialScrollVelocity >= 0;
        else
            forward = ScrollGroup.ScrollVelocities[i - 1].Multiplier >= 0;

        // Loop over SV changes between startTime and endTime.
        for (; i < ScrollGroup.ScrollVelocities.Count && endTime >= ScrollGroup.ScrollVelocities[i].StartTime; i++)
        {
            var multiplier = ScrollGroup.ScrollVelocities[i].Multiplier;
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
                StartTime = ScrollGroup.ScrollVelocities[i].StartTime, Position = VelocityPositionMarkers[i]
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
        for (i = 0; i < ScrollGroup.ScrollVelocities.Count; i++)
        {
            if (time < ScrollGroup.ScrollVelocities[i].StartTime)
                break;
        }

        i--;

        // Find index of the last non-zero SV.
        for (; i >= 0; i--)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (ScrollGroup.ScrollVelocities[i].Multiplier != 0)
                break;
        }

        if (i == -1)
            return ScrollGroup.InitialScrollVelocity < 0;

        return ScrollGroup.ScrollVelocities[i].Multiplier < 0;
    }

    public override void HandleSkip()
    {
        CurrentSvIndex = 0;
    }

    /// <summary>
    ///     Update Current position of the hit objects
    /// </summary>
    /// <param name="audioTime"></param>
    public override void UpdateCurrentTrackPosition()
    {
        // Update SV index if necessary. Afterwards update Position.
        while (CurrentSvIndex < ScrollGroup.ScrollVelocities.Count &&
               Manager.CurrentVisualAudioOffset >= ScrollGroup.ScrollVelocities[CurrentSvIndex].StartTime)
        {
            CurrentSvIndex++;
        }

        CurrentTrackPosition = GetPositionFromTime(Manager.CurrentVisualAudioOffset, CurrentSvIndex);
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