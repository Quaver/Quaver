using System;
using System.IO;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorChangeBackgroundBannerDialog : BackgroundBannerDialog
    {
        public EditorChangeBackgroundBannerDialog(EditScreen screen, string file) : base("CHANGE BACKGROUND / BANNER",
            $"Would you like to change the background or the banner?")
        {
            BackgroundAction += () =>
            {
                var name = Path.GetFileName(file);

                try
                {
                    if (ConfigManager.SongDirectory == null)
                        throw new InvalidOperationException("Song directory is null. Visual testing?");

                    try
                    {
                        File.Copy(file, $"{ConfigManager.SongDirectory.Value}/{screen.Map.Directory}/{name}", true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

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

            BannerAction += () =>
            {
                var name = Path.GetFileName(file);

                try
                {
                    if (ConfigManager.SongDirectory == null)
                        throw new InvalidOperationException("Song directory is null. Visual testing?");

                    try
                    {
                        File.Copy(file, $"{ConfigManager.SongDirectory.Value}/{screen.Map.Directory}/{name}", true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    screen.WorkingMap.BannerFile = name;
                    screen.Map.BannerPath = name;
                    screen.Save(true, true);

                    NotificationManager.Show(NotificationLevel.Success, "Your banner has been successfully changed!");
                    BackgroundHelper.Load(screen.Map);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "There was an issue while changing your banner");
                }
            };
        }
    }
}