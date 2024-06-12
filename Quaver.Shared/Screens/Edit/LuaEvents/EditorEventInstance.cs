using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Edit.Actions;

namespace Quaver.Shared.Screens.Edit.LuaEvents;

[MoonSharpUserData]
public record EditorEventInstance(IEditorAction Action, bool IsUndo)
{
    public EditorActionType ActionType => Action.Type;
}