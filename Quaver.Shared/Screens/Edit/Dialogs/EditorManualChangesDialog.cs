using System;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorManualChangesDialog : YesNoDialog
    {
        public EditorManualChangesDialog(EditScreen screen, Action onDismiss) : base("DETECTED MANUAL FILE CHANGES",
            "There were manual changes detected to the .qua file.\nWould you like to reload the editor?", () =>
            {
                screen.ReloadFromManualChanges();
            }, onDismiss)
        {
        }
    }
}
