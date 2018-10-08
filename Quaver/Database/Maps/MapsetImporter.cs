using System;
using System.IO;
using System.Linq;
using Quaver.API.Replays;
using Quaver.Config;
using Quaver.Graphics.Notifications;
using Quaver.Parsers.Etterna;
using Quaver.Parsers.Osu;
using Quaver.Scheduling;
using Quaver.Screens.Results;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using Wobble;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Database.Maps
{
    public static class MapsetImporter
    {
        /// <summary>
        ///     If the import queue has maps ready to be imported.
        /// </summary>
        public static bool QueueReady { get; private set; }

        /// <summary>
        /// Watches the songs directory for any changes.
        /// </summary>
        internal static void WatchForChanges()
        {
            // Watch the song's directory for changes.
            var watcher = new FileSystemWatcher(ConfigManager.SongDirectory.Value)
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                               NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnDirectoryChange;
            watcher.Created += OnDirectoryChange;
            watcher.Deleted += OnDirectoryChange;
            watcher.Renamed += OnDirectoryChange;
        }

        /// <summary>
        /// If there were any changes in the directory, we'll make sure to set a flag that the maps
        /// need to be reprocessed later on.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnDirectoryChange(object source, FileSystemEventArgs e)
        {
            if (!QueueReady)
                Logger.Debug($"Detected directory change at: {e.FullPath}", LogType.Runtime);

            QueueReady = true;
        }

        /// <summary>
        ///     Responsible for extracting the files from the .qp
        /// </summary>
        /// <param name="fileName"></param>
        internal static void Import(string fileName)
        {
            var extractPath = $@"{ConfigManager.SongDirectory}/{Path.GetFileNameWithoutExtension(fileName)}/";

            try
            {

                using (var archive = ArchiveFactory.Open(fileName))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(extractPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Reloads the maps properly
        /// </summary>
        /// <returns></returns>
        internal static void AfterImport()
        {
            var oldMaps = MapManager.Mapsets;

            // Import all the maps to the db
            MapCache.LoadAndSetMapsets();

            // Update the selected map with the new one.
            // This button should only be on the song select state, so no need to check for states here.
            var newMapsets = MapManager.Mapsets.Where(x => oldMaps.All(y => y.Directory != x.Directory)).ToList();

            // In the event that the user imports maps when there weren't any maps previously.
            if (oldMaps.Count == 0)
            {
            }
            else if (newMapsets.Count > 0)
            {
                var map = newMapsets.Last().Maps.Last();
                map.ChangeSelected();
            }

            NotificationManager.Show(NotificationLevel.Success, "Successfully imported mapset!");
            QueueReady = false;
        }

        /// <summary>
        ///     Allows files to be dropped into the window.
        /// </summary>
        internal static void OnFileDropped(object sender, string e)
        {
            // TODO: Don't perform these actions automatically. Wait until an actual good time
            // TODO: Since some require screen/map changes.

            // Quaver Mapset
            if (e.EndsWith(".qp"))
            {
                NotificationManager.Show(NotificationLevel.Info, "Importing mapset - please wait...");

                Scheduler.RunThread(() =>
                {
                    try
                    {
                        Import(e);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, "There was an issue when trying to import that mapset.");
                    }

                    AfterImport();
                });
            }
            // Quaver Replay
            else if (e.EndsWith(".qr"))
            {
                try
                {
                    ScreenManager.ChangeScreen(new ResultsScreen(new Replay(e)));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Error reading replay file.");
                }
            }
            // osu! beatmap archive
            else if (e.EndsWith(".osz"))
            {
                NotificationManager.Show(NotificationLevel.Info, "Importing .osz file - please wait...");

                Scheduler.RunThread(() =>
                {
                    try
                    {
                        Osu.ConvertOsz(e, 0);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, "There was an issue when trying to import that mapset.");
                    }

                    AfterImport();
                });
            }
            // StepMania file
            else if (e.EndsWith(".sm"))
            {
                NotificationManager.Show(NotificationLevel.Info, "Importing .sm file - please wait...");

                Scheduler.RunThread(() =>
                {
                    try
                    {
                        StepManiaConverter.ConvertSm(e);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, "There was an issue when trying to import that mapset.");
                    }

                    AfterImport();
                });
            }
        }
    }
}