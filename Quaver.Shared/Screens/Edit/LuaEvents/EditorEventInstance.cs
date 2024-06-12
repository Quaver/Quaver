using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Edit.Actions;

namespace Quaver.Shared.Screens.Edit.LuaEvents;

[MoonSharpUserData]
public record EditorEventInstance(IEditorAction Action, bool IsUndo)
{
    /// <summary>
    ///     Type of the action that yields the event
    /// </summary>
    public EditorActionType ActionType => Action.Type;
}