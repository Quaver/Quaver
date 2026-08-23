using System;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class UnsavedChangesNewMapDialog : YesNoDialog
    {
        public UnsavedChangesNewMapDialog(EditScreen screen, bool copyCurrent) : base(
            LocalizationManager.Get("Screen_Editor_SaveChanges"),
            LocalizationManager.Get("Screen_Editor_SaveBeforeCreatingDifficulty"))
        {
            YesAction += () =>
            {
                screen.Save(true);
                NotificationManager.Show(NotificationLevel.Success,
                    LocalizationManager.Get("Screen_Editor_MapSavedSuccessfully"));

                screen.CreateNewDifficulty(copyCurrent);
            };

            NoAction += () => screen.CreateNewDifficulty(copyCurrent, true);
        }
    }
}
