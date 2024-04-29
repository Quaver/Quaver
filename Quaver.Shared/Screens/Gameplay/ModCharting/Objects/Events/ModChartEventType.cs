using System;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

/// <summary>
///     The first 16 MSB is the category, and the last 32 LSB is the specific type.
///     Bits 16 to 31 are reserved for future use
///     When the last 16 LSB is not set (i.e. 0), it specifies the entire category of events.
/// </summary>
[Flags]
public enum ModChartEventType : ulong
{
    None = 0,
    Custom = 1UL << 32,
    /// <summary>
    ///     Note becoming visible/invisible
    /// </summary>
    Note = 1UL << 33,
    NoteEntry,
    NoteLeave,
    NoteDead,
    NoteDestroy,
    /// <summary>
    ///     User inputs something
    /// </summary>
    Input = 1UL << 34,
    InputKeyPress,
    InputKeyRelease,
    /// <summary>
    ///     Just queueing to call a function. The first argument will be the closure to call
    /// </summary>
    Function = 1UL << 35,
    FunctionCall,
    StateMachine = 1UL << 36,
    StateMachineTransition,
    Timeline = 1UL << 37,
    TimelineAddSegment,
    TimelineRemoveSegment,
    TimelineUpdateSegment,
    TimelineTrigger,
    TimelineAddTrigger,
    TimelineRemoveTrigger
}