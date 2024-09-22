using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;

/// <summary>
///     Controller for <see cref="TNoteController"/>, and operates on <see cref="THitObject"/> as game object
/// </summary>
/// <typeparam name="THitObject">Type of individual object to be controlled</typeparam>
/// <typeparam name="TNoteController">Type of the individual controller</typeparam>
public abstract class TimingGroupController<THitObject, TNoteController> where TNoteController : NoteController
{
    public TimingGroupController(TimingGroup timingGroup, Qua map)
    {
        TimingGroup = timingGroup;
        Map = map;
    }

    protected Qua Map { get; }

    /// <summary>
    ///     Timing Group associated with this controller
    /// </summary>
    public TimingGroup TimingGroup { get; }

    public abstract TNoteController CreateNoteController(THitObject hitObject, bool render = true);
}