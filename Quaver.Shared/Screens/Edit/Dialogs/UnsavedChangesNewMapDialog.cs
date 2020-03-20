using System;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class UnsavedChangesNewMapDialog : YesNoDialog
    {
        public UnsavedChangesNewMapDialog(EditScreen screen, bool copyCurrent) : base("SAVE CHANGES",
            "You have unsaved changes. Would you like to save\n" +
            "before creating a new difficulty?")
        {
            YesAction += () =>
            {
                screen.Save(true);
                NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");

                screen.CreateNewDifficulty(copyCurrent);
            };

            NoAction += () => screen.CreateNewDifficulty(copyCurrent, true);
        }
    }
}