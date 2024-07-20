// SPDX-License-Identifier: MPL-2.0
namespace Quaver.Shared.Scripting;

/// <summary>
///     Represents the type of changes done on the edit stack within editor actions.
/// </summary>
public enum EditorActionEvent
{
    /// <summary>
    ///     Indicates the editor action is a new edit that is pushed to the stack.
    /// </summary>
    New,

    /// <summary>
    ///     Indicates the editor action is a redo that came from the stack.
    /// </summary>
    Redo,

    /// <summary>
    ///     Indicates the editor action is an undo that came from the stack.
    /// </summary>
    Undo,
}
