using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Config;
using Quaver.Converters.Osu;
using Quaver.Converters.StepMania;
using Quaver.Graphics.Notifications;
using Quaver.Scheduling;
using Quaver.Screens;
using Quaver.Screens.Importing;
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
        ///     List of file paths that are ready to be imported to the game.
        /// </summary>
        public static List<string> Queue { get; } = new List<string>();

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
        }

        /// <summary>
        ///     Allows files to be dropped into the window.
        /// </summary>
        internal static void OnFileDropped(object sender, string e)
        {
            // Mapset files
            if (e.EndsWith(".qp") || e.EndsWith(".osz") || e.EndsWith(".sm"))
            {
                Queue.Add(e);

                var log = $"Scheduled {Path.GetFileName(e)} to be imported!";
                NotificationManager.Show(NotificationLevel.Info, log);

                var game = GameBase.Game as QuaverGame;
                var screen = game.CurrentScreen;

                // If in song select, automatically go to the import screen
                if (screen.Type != QuaverScreenType.Select || screen.Exiting)
                    return;

                screen.Exit(() => new ImportingScreen());
            }
            // Quaver Replay
            else if (e.EndsWith(".qr"))
            {
                try
                {
                    QuaverScreenManager.ChangeScreen(new ResultsScreen(new Replay(e)));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Error reading replay file.");
                }
            }
        }

        /// <summary>
        ///     Goes through all the mapsets in the queue and imports them.
        /// </summary>
        public static void ImportMapsetsInQueue()
        {
            Map selectedMap = null;

            if (MapManager.Selected.Value != null)
                selectedMap = MapManager.Selected.Value;

            foreach (var file in Queue)
            {
                var time = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;
                var extractDirectory = $@"{ConfigManager.SongDirectory}/{Path.GetFileNameWithoutExtension(file)} - {time}/";

                try
                {
                    if (file.EndsWith(".qp"))
                    {
                        ExtractQuaverMapset(file, extractDirectory);
                        File.Delete(file);
                    }
                    else if (file.EndsWith(".osz"))
                    {
                        Osu.ConvertOsz(file, extractDirectory);
                        File.Delete(file);
                    }
                    else if (file.EndsWith(".sm"))
                        Stepmania.ConvertFile(file, extractDirectory);

                    selectedMap = InsertAndUpdateSelectedMap(extractDirectory);

                    Logger.Important($"Successfully imported {file}", LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Success, $"Successfully imported file: {Path.GetFileName(file)}");
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            MapDatabaseCache.OrderAndSetMapsets();

            var mapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Md5Checksum == selectedMap?.Md5Checksum));
            MapManager.Selected.Value =MapManager.Selected.Value = mapset.Maps.Find(x => x.Md5Checksum == selectedMap?.Md5Checksum);

            Queue.Clear();
        }

        /// <summary>
        ///     Responsible for extracting the files from the .qp
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extractPath"></param>
        private static void ExtractQuaverMapset(string fileName, string extractPath)
        {
            try
            {
                using (var archive = ArchiveFactory.Open(fileName))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                            entry.WriteToDirectory(extractPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Inserts an entire extracted directory of maps to the DB
        /// </summary>
        /// <param name="extractDirectory"></param>
        private static Map InsertAndUpdateSelectedMap(string extractDirectory)
        {
            // Go through each file in the directory and import it into the database.
            var quaFiles = Directory.GetFiles(extractDirectory, "*.qua", SearchOption.AllDirectories).ToList();
            Map lastImported = null;

            try
            {
                foreach (var quaFile in quaFiles)
                {
                    var map = Map.FromQua(Qua.Parse(quaFile), quaFile);
                    map.CalculateDifficulties();
                    MapDatabaseCache.InsertMap(map, quaFile);
                    lastImported = map;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            return lastImported;
        }
    }
}