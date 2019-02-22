/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Replays;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Converters.Osu;
using Quaver.Shared.Converters.StepMania;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Result;
using Quaver.Shared.Skinning;
using SharpCompress.Archives;
using SharpCompress.Common;
using Wobble;
using Wobble.Logging;

namespace Quaver.Shared.Database.Maps
{
    public static class MapsetImporter
    {
        /// <summary>
        ///     List of file paths that are ready to be imported to the game.
        /// </summary>
        public static List<string> Queue { get; } = new List<string>();

        /// <summary>
        ///     Invoked when a map gets initialized for importing
        /// </summary>
        public static event EventHandler<ImportingMapsetEventArgs> ImportingMapset;

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
            var game = GameBase.Game as QuaverGame;
            var screen = game.CurrentScreen;

            // Mapset files
            if (e.EndsWith(".qp") || e.EndsWith(".osz") || e.EndsWith(".sm"))
            {
                Queue.Add(e);

                var log = $"Scheduled {Path.GetFileName(e)} to be imported!";
                NotificationManager.Show(NotificationLevel.Info, log);

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
                    switch (screen.Type)
                    {
                        case QuaverScreenType.Menu:
                        case QuaverScreenType.Results:
                        case QuaverScreenType.Select:
                            break;
                        default:
                            NotificationManager.Show(NotificationLevel.Error, "Please exit this screen before loading a replay");
                            return;
                    }

                    var replay = new Replay(e);

                    // Find the map associated with the replay.
                    var mapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Md5Checksum == replay.MapMd5));

                    if (mapset == null)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You do not have the map associated with this replay.");
                        return;
                    }

                    MapManager.Selected.Value = mapset.Maps.Find(x => x.Md5Checksum == replay.MapMd5);

                    screen.Exit(() =>
                    {
                        if (AudioEngine.Track != null)
                        {
                            lock (AudioEngine.Track)
                                AudioEngine.Track.Fade(10, 300);
                        }

                        return new ResultScreen(replay);
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Error reading replay file.");
                }
            // Skins
            }
            else if (e.EndsWith(".qs"))
            {
                switch (screen.Type)
                {
                    case QuaverScreenType.Menu:
                    case QuaverScreenType.Results:
                    case QuaverScreenType.Select:
                        SkinManager.Import(e);
                        break;
                    default:
                        NotificationManager.Show(NotificationLevel.Error, "Please exit this screen before importing a skin");
                        return;
                }
            }
            else if (e.EndsWith(".mp3") || e.EndsWith(".ogg"))
            {
                switch (screen.Type)
                {
                    case QuaverScreenType.Select:
                        EditorScreen.HandleNewMapsetCreation(e);
                        break;
                    default:
                        NotificationManager.Show(NotificationLevel.Error, "Go to the song select screen first to create a new mapset!");
                        return;
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

            for (var i = 0; i < Queue.Count; i++)
            {
                var file = Queue[i];
                var time = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;
                var extractDirectory = $@"{ConfigManager.SongDirectory}/{Path.GetFileNameWithoutExtension(file)} - {time}/";
                ImportingMapset.Invoke(typeof(MapsetImporter), new ImportingMapsetEventArgs(Queue, Path.GetFileName(file), i));

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
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, $"Failed to import file: {Path.GetFileName(file)}");
                }
            }

            MapDatabaseCache.OrderAndSetMapsets();

            var mapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Md5Checksum == selectedMap?.Md5Checksum));

            if (mapset == null)
            {
                mapset = MapManager.Mapsets.First();
                MapManager.Selected.Value = mapset.Maps.First();
            }
            else
                MapManager.Selected.Value = mapset.Maps.Find(x => x.Md5Checksum == selectedMap?.Md5Checksum);

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
                    map.DifficultyProcessorVersion = DifficultyProcessorKeys.Version;
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
