using System;
using Quaver.Shared.Graphics.Dialogs;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class EditorSaveAndQuitDialog : ConfirmCancelDialog
    {
        /// <summary>
        /// </summary>
        private EditorScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorSaveAndQuitDialog(EditorScreen screen): base("Would you like to save this map before quitting?", null)
        {
            Screen = screen;
            SureButton.Clicked += (sender, args) => Screen.Save(true);
        }
    }
}