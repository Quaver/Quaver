using System;
using System.IO;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorChangeBackgroundDialog : YesNoDialog
    {
        public EditorChangeBackgroundDialog(EditScreen screen, string file) : base("CHANGE BACKGROUND",
            $"Are you sure you would like to change the background?")
        {
            YesAction += () =>
            {
                var name = Path.GetFileName(file);

                try
                {
                    if (ConfigManager.SongDirectory == null)
                        throw new InvalidOperationException("Song directory is null. Visual testing?");

                    File.Copy(file, $"{ConfigManager.SongDirectory.Value}/{screen.Map.Directory}/{name}", true);

                    screen.WorkingMap.BackgroundFile = name;
                    screen.Map.BackgroundPath = name;
                    screen.Save(true, true);

                    NotificationManager.Show(NotificationLevel.Success, "Your background has been successfully changed!");
                    BackgroundHelper.Load(screen.Map);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "There was an issue while changing your background");
                }
            };
        }
    }
}