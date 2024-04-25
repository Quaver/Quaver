namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

/// <summary>
///     The 16 LSB is the specific type, and the rest 48 MSB is the category
/// </summary>
public enum ModChartEventType : ulong
{
    None = 0,
    Custom = 1 << 16,
    /// <summary>
    ///     Note becoming visible/invisible
    /// </summary>
    Note = 1 << 17,
    NoteEntry,
    NoteLeave,
    NoteDead,
    NoteDestroy,
    /// <summary>
    ///     User inputs something
    /// </summary>
    Input = 1 << 18,
    InputKeyPress,
    InputKeyRelease,
    /// <summary>
    ///     Just queueing to call a function. The first argument will be the closure to call
    /// </summary>
    Function = 1 << 19,
    FunctionCall,
}