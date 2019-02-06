using System;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Screens.Settings;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class EditorUploadConfirmationDialog : ConfirmCancelDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorUploadConfirmationDialog() : base("Are you sure you want to upload your mapset?", OnConfirm)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnConfirm(object sender, EventArgs e)
        {
            DialogManager.Show(new EditorUploadMapsetDialog());
        }
    }
}