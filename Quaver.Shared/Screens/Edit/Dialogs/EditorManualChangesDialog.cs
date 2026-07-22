using System;
using Quaver.Shared.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorManualChangesDialog : YesNoDialog
    {
        public EditorManualChangesDialog(EditScreen screen, Action onDismiss) : base(
            LocalizationManager.Get("Screen_Editor_DetectedManualFileChanges"),
            LocalizationManager.Get("Screen_Editor_DetectedManualFileChangesMessage"), () =>
            {
                screen.ReloadFromManualChanges();
            }, onDismiss)
        {
        }
    }
}
