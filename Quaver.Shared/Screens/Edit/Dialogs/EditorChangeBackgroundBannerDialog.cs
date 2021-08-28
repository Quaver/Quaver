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
            BackgroundBannerAction += () =>
            {
                var name = Path.GetFileName(file);

                if (ConfigManager.SongDirectory == null)
                    throw new InvalidOperationException("Song directory is null. Visual testing?");

                try
                {
                    File.Copy(file, $"{ConfigManager.SongDirectory.Value}/{screen.Map.Directory}/{name}", true);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "An error occured while moving the file.");
                }

                BackgroundAction += () =>
                {
                    try
                    {
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
                    try
                    {
                        screen.WorkingMap.BannerFile = name;
                        foreach (var Map in screen.Map.Mapset.Maps) {
                            var QuaFile = Map.LoadQua();
                            Map.BannerPath = name;
                            QuaFile.BannerFile = name;
                            QuaFile.Save($"{ConfigManager.SongDirectory}/{Map.Directory}/{Map.Path}");
                        }
                        screen.Save(true, true, true);
                        BackgroundHelper.MapsetBannersToLoad.Add(screen.Map.Mapset);
                        BackgroundHelper.LoadAllMapsetBanners(true);

                        NotificationManager.Show(NotificationLevel.Success, "Your banner has been successfully changed!");
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, "There was an issue while changing your banner");
                    }
                };
            };
        }
    }
}