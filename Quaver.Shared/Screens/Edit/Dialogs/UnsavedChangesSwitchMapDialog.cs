using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class UnsavedChangesSwitchMapDialog : YesNoDialog
    {
        public UnsavedChangesSwitchMapDialog(EditScreen screen, Map map) : base("SAVE CHANGES",
            "You have unsaved changes. Would you like to save\n" +
            "before switching to another difficulty?")
        {
            YesAction += () =>
            {
                screen.Save(true);
                NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");

                screen.SwitchToMap(map);
            };

            NoAction += () => screen.SwitchToMap(map, true);
        }
    }
}