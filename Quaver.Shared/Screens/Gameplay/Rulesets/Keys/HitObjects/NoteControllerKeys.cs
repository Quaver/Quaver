using System;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

/// <summary>
///     Controls a <see cref="HitObjectInfo"/>
/// </summary>
public abstract class NoteControllerKeys : NoteController
{
    /// <summary>
    ///     Whether a hit object has been hit, held, missed, or not yet hit
    /// </summary>
    private HitObjectState _state;

    public HitObjectState State
    {
        get => _state;
        set
        {
            if (value == HitObjectState.Dead && HitObject != null)
                HitObject.Kill();

            _state = value;
        }
    }

    /// <summary>
    ///     Represents the hitobject on screen
    /// </summary>
    public GameplayHitObjectKeys HitObject { get; private set; }

    /// <summary>
    ///     Original hitobject info
    /// </summary>
    public HitObjectInfo HitObjectInfo { get; private set; }

    protected NoteControllerKeys(HitObjectInfo hitObjectInfo, TimingGroupControllerKeys timingGroupController,
        GameplayHitObjectKeys hitObject)
    {
        HitObjectInfo = hitObjectInfo;
        TimingGroupController = timingGroupController;
        HitObject = hitObject;
        Manager = TimingGroupController.Manager;
    }

    public int StartTime => HitObjectInfo.StartTime;
    public int Lane => HitObjectInfo.Lane;
    public int EndTime => HitObjectInfo.EndTime;
    public bool IsLongNote => HitObjectInfo.IsLongNote;

    /// <summary>
    ///     Reference to the hitobject manager that this belongs to
    /// </summary>
    /// <value></value>
    protected HitObjectManagerKeys Manager { get; set; }

    public TimingGroupControllerKeys TimingGroupController { get; protected set; }


    /// <summary>
    ///     Gets the value determining whether to use the old LN rendering system. (earliest/latest -> start/end)
    /// </summary>
    public bool LegacyLNRendering => Manager.LegacyLNRendering;

    /// <summary>
    ///     Y-offset from the origin
    /// </summary>
    public long InitialTrackPosition { get; protected set; }

    /// <summary>
    ///     Position of the LN end sprite.
    /// </summary>
    public long EndTrackPosition { get; protected set; }

    /// <summary>
    ///     Latest position of this object.
    ///
    ///     For LNs with negative SVs, this can be larger than EndTrackPosition.
    /// </summary>
    public long LatestTrackPosition { get; protected set; }

    /// <summary>
    ///     Earliest position of this object.
    ///
    ///     For LNs with negative SVs, this can be earlier than InitialTrackPosition.
    /// </summary>
    public long EarliestTrackPosition { get; protected set; }

    /// <summary>
    ///     Earliest position of this object, can change while the long note is held.
    ///     Note that this is different from <see cref="Info"/>'s EarliestTrackPosition.
    /// </summary>
    public long EarliestHeldPosition { get; protected set; }

    /// <summary>
    ///     Latest position of this object, can change while the long note is held.
    ///     Note that this is different from <see cref="Info"/>'s LatestTrackPosition.
    /// </summary>
    public long LatestHeldPosition { get; protected set; }

    /// <summary>
    ///     If the long note end should be flipped up-side down
    /// </summary>
    public abstract bool ShouldFlipLongNoteEnd { get; }

    /// <summary>
    ///     Returns the long note body size
    /// </summary>
    public abstract float CurrentLongNoteBodySize { get; }

    public abstract void UpdateLongNoteSize(double curTime);

    /// <summary>
    ///     Associate a GameplayHitObjectKeys to start drawing this hitobject.
    /// </summary>
    /// <param name="hitObject"></param>
    public void Link(GameplayHitObjectKeys hitObject)
    {
        HitObject = hitObject;
        HitObject.InitializeObject(Manager, this);
    }

    /// <summary>
    ///     Dissaociate the currently linked GameplayHitObjectKeys to stop drawing this hitobject.
    /// </summary>
    /// <returns>The linked GameplayHitObjectKeys to add back to the pool.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public GameplayHitObjectKeys Unlink()
    {
        if (HitObject is null)
            throw new InvalidOperationException("GameplayHitObjectInfo is not linked to a GameplayHitObjectKeys");

        HitObject.Hide();
        var temp = HitObject;
        HitObject = null;
        return temp;
    }

    public abstract float GetSpritePosition(float hitPosition, float initialPos);
    public abstract void InitializeLongNoteSize();
}

/// <summary>
///     Hitobjects can be alive, held, dead, or removed.
/// </summary>
public enum HitObjectState
{
    /// <summary>
    ///     Hitobject has not been hit yet.
    ///     Sprite should appear as normal.
    /// </summary>
    Alive,

    /// <summary>
    ///     Hitobject is a long note and is currently being held.
    ///     Long note length should be changing over time.
    /// </summary>
    Held,

    /// <summary>
    ///     Note has been late-missed,
    ///     or long note has been early or late-missed,
    ///     or long note was released very early.
    ///     Sprite should be tinted to show that it was missed.
    /// </summary>
    Dead,

    /// <summary>
    ///     The note has been hit, or the long note has been properly released.
    ///     There should no longer be a visible sprite representing the hitobject.
    /// </summary>
    Removed
}