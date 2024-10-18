using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble;
using Wobble.Window;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

/// <summary>
///     Group controller for the keys game mode
/// </summary>
public abstract class TimingGroupControllerKeys : TimingGroupController<HitObjectInfo, NoteControllerKeys>
{
    /// <summary>
    /// </summary>
    public HitObjectManagerKeys Manager { get; }


    /// <summary>
    ///     The speed at which objects travel across the screen.
    /// </summary>
    public virtual float ScrollSpeed
    {
        get
        {
            var speed = ConfigManager.ScrollSpeed4K;

            if (MapManager.Selected.Value.Qua != null)
                speed = MapManager.Selected.Value.Qua.Mode == GameMode.Keys4
                    ? ConfigManager.ScrollSpeed4K
                    : ConfigManager.ScrollSpeed7K;

            var scalingFactor = QuaverGame.SkinScalingFactor;

            var game = GameBase.Game as QuaverGame;

            if (game?.CurrentScreen is IHasLeftPanel)
                scalingFactor = (1920f - GameplayPlayfieldKeys.PREVIEW_PLAYFIELD_WIDTH) / 1366f;

            var scrollSpeed = (speed.Value / 10f) / (20f * AudioEngine.Track.Rate) * scalingFactor *
                              WindowManager.BaseToVirtualRatio;

            return scrollSpeed;
        }
    }

    /// <summary>
    /// </summary>
    public GameplayRulesetKeys Ruleset => Manager.Ruleset;

    /// <summary>
    /// </summary>
    public abstract long RenderThreshold { get; }

    /// <summary>
    ///     Current position of the receptors
    /// </summary>
    public long CurrentTrackPosition { get; protected set; }

    /// <summary>
    ///     List of <see cref="NoteControllerKeys"/> that are in this group
    /// </summary>
    protected List<NoteControllerKeys> NoteControllersToRender { get; } = new();

    /// <summary>
    ///     Really long LNs that would take up all the memory in the universe if they were added to the spatial hash map.
    /// </summary>
    protected HashSet<NoteControllerKeys> LongLNs { get; private set; }

    /// <summary>
    ///     Allows for quickly finding hitobjects close to some position.
    /// </summary>
    public SpatialHashMap<NoteControllerKeys> SpatialHashMap { get; private set; }

    public TimingGroupControllerKeys(TimingGroup timingGroup, Qua map, HitObjectManagerKeys manager) : base(timingGroup,
        map)
    {
        Manager = manager;
    }

    public virtual void Initialize()
    {
        LongLNs = new HashSet<NoteControllerKeys>();

        // Using cell size equal to render area guarantees a consistent two cells accessed per update
        SpatialHashMap = new SpatialHashMap<NoteControllerKeys>(2 * RenderThreshold);
    }

    /// <summary>
    ///     Fill in the <see cref="SpatialHashMap"/> and <see cref="LongLNs"/>
    /// </summary>
    public void GenerateFromNoteControllers()
    {
        foreach (var info in NoteControllersToRender)
        {
            if (!info.IsLongNote)
            {
                SpatialHashMap.Add(info.InitialTrackPosition, info);
            }
            else
            {
                // Long LNs need to be added to multiple cells.
                // If they're _really_ long, they need to be added a _really_ large number of times to the spatial hash map,
                // which would require an insane amount of memory, so don't do that.
                // Negative length LNs (such as those in Cheat Code) cause the same problem.
                long length = info.LatestTrackPosition - info.EarliestTrackPosition;
                if (length > SpatialHashMap.CellSize * 10 || length < 0)
                {
                    LongLNs.Add(info);
                }
                else
                {
                    SpatialHashMap.Add(info.EarliestTrackPosition, info.LatestTrackPosition, info);
                }
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public abstract long GetPositionFromTime(double time);

    /// <summary>
    ///     Updates <see cref="CurrentTrackPosition"/>
    /// </summary>
    public abstract void UpdateCurrentTrackPosition();

    /// <summary>
    ///     When called, it means that a skip has occurred. This can be implemented to reset something.
    /// </summary>
    public abstract void HandleSkip();

    /// <summary>
    ///     Add <see cref="HitObjectInfo"/>s that are in range
    /// </summary>
    /// <param name="inRangeHitObjectInfos"></param>
    public void UnionInRangeHitObjectInfos(HashSet<NoteControllerKeys> inRangeHitObjectInfos)
    {
        // find hitobjects in all visible cells
        for (long position = CurrentTrackPosition - RenderThreshold;
             position < CurrentTrackPosition + RenderThreshold;
             position += SpatialHashMap.CellSize)
        {
            inRangeHitObjectInfos.UnionWith(SpatialHashMap.GetValues(position));
        }

        inRangeHitObjectInfos.UnionWith(SpatialHashMap.GetValues(CurrentTrackPosition + RenderThreshold));

        // really long LNs aren't added to the spatial hash map to avoid using all the memory in the universe
        inRangeHitObjectInfos.UnionWith(LongLNs);
    }

    /// <summary>
    ///     Determines if a note controller is in the rendering range
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool InRange(NoteControllerKeys info)
    {
        if (!info.IsLongNote)
        {
            return Math.Abs(info.InitialTrackPosition - CurrentTrackPosition) <= RenderThreshold;
        }
        else
        {
            if (Math.Abs(info.EarliestTrackPosition - CurrentTrackPosition) <= RenderThreshold)
                return true;

            if (Math.Abs(info.LatestTrackPosition - CurrentTrackPosition) <= RenderThreshold)
                return true;

            if (info.EarliestTrackPosition <= CurrentTrackPosition - RenderThreshold &&
                CurrentTrackPosition - RenderThreshold <= info.LatestTrackPosition)
                return true;

            if (info.EarliestTrackPosition <= CurrentTrackPosition + RenderThreshold &&
                CurrentTrackPosition + RenderThreshold <= info.LatestTrackPosition)
                return true;

            return false;
        }
    }
}