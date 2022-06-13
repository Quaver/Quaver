using System;
using System.IO;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorChangeBackgroundDialog : YesNoDialog
    {
        private IconButton BannerButton { get; }

        /// <param name="screen"></param>
        /// <param name="file"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public EditorChangeBackgroundDialog(EditScreen screen, string file) : base("CHANGE BACKGROUND / BANNER",
            $"Would you like to change the background or the banner?")
        {
            YesButton.Image = UserInterface.EditorBackgroundButton;
            YesAction += () => ChangeBackground(screen, file);

            const float scale = 0.90f;
            YesButton.Size = new ScalableVector2(YesButton.Width * scale, YesButton.Height * scale);
            NoButton.Size = new ScalableVector2(NoButton.Width * scale, NoButton.Height * scale);

            BannerButton = new IconButton(UserInterface.EditorBannerButton, (sender, args) => ChangeBanner(screen, file))
            {
                Parent = Panel,
                Size = YesButton.Size,
                Alignment = Alignment.BotCenter,
                Y = YesButton.Y,
            };

            YesButton.X -= 80;
            NoButton.X = -YesButton.X;
        }

        /// <summary>
        /// </summary>
        public override void Close()
        {
            BannerButton.IsClickable = false;
            BannerButton.IsPerformingFadeAnimations = false;
            base.Close();
        }

        /// <summary>
        ///     Changes the background file of the map
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="file"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ChangeBackground(EditScreen screen, string file)
        {
            try
            {
                var name = Path.GetFileName(file);
                var backgroundFile = screen.WorkingMap.BackgroundFile;
                CopyFileToFolder(screen, file);

                // Removes existing map background file if it exists.
                if (!string.IsNullOrEmpty(backgroundFile))
                {
                    foreach (var map in screen.Map.Mapset.Maps)
                    {
                        if (map.Id == screen.Map.Id)
                            continue;

                        var qua = map.LoadQua();

                        // If background is in use by another map in the mapset, don't remove it.
                        if (qua.GetBackgroundPath() != null && qua.BackgroundFile == backgroundFile)
                            break;

                        File.Delete($"{ConfigManager.SongDirectory}/{screen.Map.Directory}/{backgroundFile}");
                    }
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
                NotificationManager.Show(NotificationLevel.Error, "There was an issue changing your background!");
            }
        }

        /// <summary>
        ///     Changes the banner file of the mapset
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="file"></param>
        private void ChangeBanner(EditScreen screen, string file)
        {
            Close();

            DialogManager.Show(new LoadingDialog("CHANGING BANNER", "Please wait while your banner is being changed...", () =>
            {
                try
                {
                    var name = Path.GetFileName(file);
                    CopyFileToFolder(screen, file);

                    // Go through each map in the set and update the banner.
                    // There's only one displayed banner, so there's no reason to have multiple files for this.
                    foreach (var map in screen.Map.Mapset.Maps)
                    {
                        // Skip the working map. This is handled manually below.
                        if (map == screen.Map)
                            continue;

                        var qua = map.LoadQua();
                        qua.BannerFile = name;
                        map.BannerPath = name;
                        qua.Save($"{ConfigManager.SongDirectory}/{map.Directory}/{map.Path}");

                        map.DifficultyProcessorVersion = "Needs Update";
                        MapDatabaseCache.UpdateMap(map);

                        if (!MapDatabaseCache.MapsToUpdate.Contains(map))
                            MapDatabaseCache.MapsToUpdate.Add(map);
                    }

                    // Get rid of the old banner if one exists
                    if (BackgroundHelper.MapsetBanners.ContainsKey(screen.Map.Directory))
                    {
                        var banner = BackgroundHelper.MapsetBanners[screen.Map.Directory];
                        banner.Dispose();
                        BackgroundHelper.MapsetBanners.Remove(screen.Map.Directory);
                    }

                    // Manually change the working map separately.
                    screen.WorkingMap.BannerFile = name;
                    screen.Map.BannerPath = name;
                    screen.Save(true, true);

                    NotificationManager.Show(NotificationLevel.Success, "Your banner has successfully changed!");
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "There was an issue changing your banner!");
                }
            }));
        }

        /// <summary>
        ///     Copies the incoming file to the mapset directory.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="file"></param>
        private void CopyFileToFolder(EditScreen screen, string file)
        {
            var name = Path.GetFileName(file);

            if (ConfigManager.SongDirectory == null)
                Logger.Error($"Song directory does not exist. Visual testing?", LogType.Runtime);

            File.Copy(file, $"{ConfigManager.SongDirectory?.Value}/{screen.Map.Directory}/{name}", true);
        }
    }
}