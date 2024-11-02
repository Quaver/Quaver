namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

/// <summary>
///     Note position and shape descriptor. This uses track and held positions.
/// </summary>
public struct NoteState
{
    /// <summary>
    ///     Y-offset from the origin
    /// </summary>
    public long InitialTrackPosition;

    /// <summary>
    ///     Position of the LN end sprite.
    /// </summary>
    public long EndTrackPosition;

    /// <summary>
    ///     Earliest position of this object, can change while the long note is held.
    /// </summary>
    public long EarliestHeldPosition;

    /// <summary>
    ///     Latest position of this object, can change while the long note is held.
    /// </summary>
    public long LatestHeldPosition;

}