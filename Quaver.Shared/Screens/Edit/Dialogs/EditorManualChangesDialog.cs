using System;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorManualChangesDialog : YesNoDialog
    {
        public EditorManualChangesDialog(EditScreen screen) : base("DETECTED MANUAL FILE CHANGES",
            "There were manual changes detected to the .qua file.\nWould you like to reload the editor?", () =>
            {
                screen.RefreshFileCache();
                screen.Exit(() => new EditScreen(screen.Map));
            })
        {
        }
    }
}