namespace Quaver.Shared.Screens.Edit.Actions
{
    public interface IEditorAction
    {
        /// <summary>
        ///     The type of action that was performed
        /// </summary>
        EditorActionType Type { get; }

        /// <summary>
        ///     Does performing logic for the action
        /// </summary>
        void Perform();

        /// <summary>
        ///     Undos the performing logic for the action
        /// </summary>
        void Undo();
    }
}