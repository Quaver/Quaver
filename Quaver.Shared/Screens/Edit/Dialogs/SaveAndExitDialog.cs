using System;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class SaveAndExitDialog : YesNoDialog
    {
        public SaveAndExitDialog(EditScreen screen) : base("EXIT EDITOR",
            "You have unsaved changes. Would you like to save?")
        {
            YesAction += () =>
            {
                screen.Save(true);
                screen.ExitToSongSelect();
            };

            NoAction += screen.ExitToSongSelect;
        }
    }
}