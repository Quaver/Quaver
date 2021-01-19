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
using System.Threading.Tasks;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Replays;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Converters.Malody;
using Quaver.Shared.Converters.Osu;
using Quaver.Shared.Converters.StepMania;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Result;
using Quaver.Shared.Screens.Results;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Skinning;
using SharpCompress.Archives;
using SharpCompress.Common;
using Wobble;
using Wobble.Logging;
using Wobble.Screens;

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
            ImportFile(e);
        }

        /// <summary>
        ///     Starts importing of queued Maps.
        /// </summary>
        private static void PostMapQueue()
        {
             var game = GameBase.Game as QuaverGame;
             var screen = game.CurrentScreen;

             if (screen.Exiting)
                    return;

                if (screen.Type == QuaverScreenType.Select)
                {
                    if (OnlineManager.CurrentGame != null)
                    {
                        var select = game.CurrentScreen as SelectionScreen;
                        screen.Exit(() => new ImportingScreen(null, true));
                        return;
                    }

                    screen.Exit(() => new ImportingScreen());
                    return;
                }

                if (screen.Type == QuaverScreenType.Music)
                {
                    screen.Exit(() => new ImportingScreen());
                    return;
                }

                if (screen.Type == QuaverScreenType.Multiplayer)
                {
                    var multi = (MultiplayerGameScreen)screen;
                    multi.DontLeaveGameUponScreenSwitch = true;

                    screen.Exit(() => new ImportingScreen());
                    return;
                }
        }

        /// <summary>
        ///     Simply returns true if the file given matches a map type which can be imported. False otherwise.
        /// </summary>
        /// <param name="path">Path to file</param>
        private static bool AcceptedMapType(string path)
        {
            return path.EndsWith(".qp") || path.EndsWith(".osz") || path.EndsWith(".sm") || path.EndsWith(".mcz") || path.EndsWith(".mc");
        }

        /// <summary>
        ///     Tries to import the given file, be it a map, a replay, a skin, etc.
        ///     <param name="path">path to the file to import</param>
        /// </summary>
        public static void ImportFile(string path)
        {
            var game = GameBase.Game as QuaverGame;
            var screen = game.CurrentScreen;

            // Mapset files (or directory of Mapset files)
            if (AcceptedMapType(path))
            {
                Queue.Add(path);

                var log = $"Scheduled {Path.GetFileName(path)} to be imported!";
                NotificationManager.Show(NotificationLevel.Info, log);

                PostMapQueue();
            }
            // Quaver Replay
            else if (path.EndsWith(".qr"))
            {
                try
                {
                    switch (screen.Type)
                    {
                        case QuaverScreenType.Menu:
                        case QuaverScreenType.Results:
                        case QuaverScreenType.Select:
                            break;
                        // Replay imports are handled by the screen itself
                        case QuaverScreenType.Theatre:
                            return;
                        default:
                            NotificationManager.Show(NotificationLevel.Error, "Please exit this screen before loading a replay");
                            return;
                    }

                    var replay = new Replay(path);

                    // Find the map associated with the replay.
                    var mapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Md5Checksum == replay.MapMd5));

                    if (mapset == null)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You do not have the map associated with this replay.");
                        return;
                    }

                    var map = mapset.Maps.Find(x => x.Md5Checksum == replay.MapMd5);

                    if (map == null)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You do not have the map associated with this replay.");
                        return;
                    }

                    MapManager.Selected.Value = map;

                    BackgroundHelper.Load(map);
                    AudioEngine.LoadCurrentTrack();
                    AudioEngine.Track?.Play();

                    screen.Exit(() => new ResultsScreen(MapManager.Selected.Value, replay));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Error reading replay file.");
                }
            // Skins
            }
            else if (path.EndsWith(".qs"))
            {
                switch (screen.Type)
                {
                    case QuaverScreenType.Menu:
                    case QuaverScreenType.Results:
                    case QuaverScreenType.Select:
                        SkinManager.Import(path);
                        break;
                    default:
                        NotificationManager.Show(NotificationLevel.Error, "Please exit this screen before importing a skin");
                        return;
                }
            }
            else if (path.EndsWith(".mp3") || path.EndsWith(".ogg"))
            {
                switch (screen.Type)
                {
                    case QuaverScreenType.Menu:
                    case QuaverScreenType.Select:
                        EditScreen.CreateNewMapset(path);
                        break;
                    case QuaverScreenType.Editor:
                        break;
                    default:
                        NotificationManager.Show(NotificationLevel.Error, "Please finish what you are doing before creating a new mapset!");
                        return;
                }
            }
            // Other-game database files
            else if (path.EndsWith(".db"))
            {
                var loadedDb = false;

                if (path.Contains("osu!.db"))
                {
                    ConfigManager.OsuDbPath.Value = path;
                    loadedDb = true;
                }

                if (path.Contains("cache.db"))
                {
                    ConfigManager.EtternaDbPath.Value = path;
                    loadedDb = true;
                }

                if (!loadedDb)
                {
                    NotificationManager.Show(NotificationLevel.Warning, $"Unable to detect supported .db file.");
                    return;
                }

                ConfigManager.AutoLoadOsuBeatmaps.Value = true;
                NotificationManager.Show(NotificationLevel.Success, $"Successfully set the path for your .db file");
            }
            // Folder with maps
            else if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                var files = Directory.GetFiles(path);
                var dirs = Directory.GetDirectories(path);

                foreach (var subPath in files)
                {
                    if (AcceptedMapType(subPath))
                    {
                        NotificationManager.Show(NotificationLevel.Info, $"Scheduled {Path.GetFileName(subPath)} to be imported!");
                        Queue.Add(subPath);
                    }
                }
                foreach (var subDir in dirs)
                {
                    MapsetImporter.ImportFile(subDir);
                }

                PostMapQueue();
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

            var done = -1;

            Parallel.For(0, Queue.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
            {
                var file = Queue[i];
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
                    else if (file.EndsWith(".mc"))
                        Malody.ExtractFile(file, extractDirectory);
                    else if (file.EndsWith(".mcz"))
                    {
                        Malody.ExtractZip(file, extractDirectory);
                        File.Delete(file);
                    }

                    selectedMap = InsertAndUpdateSelectedMap(extractDirectory);

                    Logger.Important($"Successfully imported {file}", LogType.Runtime);

                    done++;
                    ImportingMapset?.Invoke(typeof(MapsetImporter), new ImportingMapsetEventArgs(Queue, Path.GetFileName(file), done));
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, $"Failed to import file: {Path.GetFileName(file)}");
                }
            });

            MapDatabaseCache.OrderAndSetMapsets();
            Queue.Clear();

            if (MapManager.Mapsets.Count == 0)
                return;

            var mapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Md5Checksum == selectedMap?.Md5Checksum));

            if (mapset == null)
            {
                mapset = MapManager.Mapsets.First();
                MapManager.Selected.Value = mapset.Maps.First();
            }
            else
                MapManager.Selected.Value = mapset.Maps.Find(x => x.Md5Checksum == selectedMap?.Md5Checksum);
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

                    var info = OnlineManager.Client?.RetrieveMapInfo(map.MapId);

                    if (info != null)
                    {
                        map.RankedStatus = info.Map.RankedStatus;
                        map.DateLastUpdated = info.Map.DateLastUpdated;
                    }

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
