/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Replays;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Converters.Malody;
using Quaver.Shared.Converters.Osu;
using Quaver.Shared.Converters.StepMania;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.Results;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Skinning;
using SharpCompress.Archives;
using SharpCompress.Common;
using Wobble;
using Wobble.Logging;
using YamlDotNet.Serialization;

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
            }
        }

        /// <summary>
        ///     Simply returns true if the file given matches a map type which can be imported. False otherwise.
        /// </summary>
        /// <param name="path">Path to file</param>
        private static bool AcceptedMapType(string path)
        {
            return path.EndsWith(".qp") || path.EndsWith(".qpl") || path.EndsWith(".osz") || path.EndsWith(".sm") || path.EndsWith(".mcz") || path.EndsWith(".mc");
        }

        /// <summary>
        ///     Adds the map dragged into the window to be scheduled to import
        /// </summary>
        /// <param name="path"></param>
        private static void AddMapImportToQueue(string path)
        {
            // Only one .mc file under the same directory should be imported
            // since .mc import
            if (Path.GetExtension(path) == ".mc")
            {
                foreach (var scheduledPath in Queue)
                {
                    if (Path.GetDirectoryName(scheduledPath) == Path.GetDirectoryName(path)) return;
                }
            }

            NotificationManager.Show(NotificationLevel.Info, $"Scheduled {Path.GetFileName(path)} to be imported!");
            Queue.Add(path);
            PostMapQueue();
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
                AddMapImportToQueue(path);
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
            // Archive with maps (batch import)
            else if (path.EndsWith(".zip"))
            {
                var time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;
                var tempFolder = $@"{ConfigManager.TempDirectory}/{Path.GetFileNameWithoutExtension(path)} - {time}";

                using (ZipArchive archive = ZipFile.OpenRead(path))
                {
                    archive.ExtractToDirectory(tempFolder);
                }

                foreach (var file in Directory.GetFiles(tempFolder, "*", SearchOption.AllDirectories))
                {
                    ImportFile(file);
                }

                // delete zip file after all maps have been extracted
                if (ConfigManager.DeleteOriginalFileAfterImport.Value)
                    File.Delete(path);
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
                        AddMapImportToQueue(subPath);
                    }
                }
                foreach (var subDir in dirs)
                {
                    ImportFile(subDir);
                }

                PostMapQueue();
            }
        }

        /// <summary>
        ///     Goes through all the mapsets in the queue and imports them.
        /// </summary>
        public static void ImportMapsetsInQueue(int? selectMapIdAfterImport = null)
        {
            Map selectedMap = null;

            if (MapManager.Selected.Value != null)
                selectedMap = MapManager.Selected.Value;

            var done = -1;

            var skipPlaylistReload = true;

            MapsetInfoRetriever.InfoUpdateEnabled = false;
            Parallel.For(0, Queue.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
            {
                var file = Queue[i];
                var extension = Path.GetExtension(file);
                // Use directory of .sm files, because during scheduled bulk import, there can be multiple files named file.sm, for example
                var isPartOfMapset = extension == ".sm";
                var time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;

                var folderName = isPartOfMapset
                    ? Path.GetFileName(Path.GetDirectoryName(file))
                    : Path.GetFileNameWithoutExtension(file);
                folderName = folderName.Substring(0, Math.Min(folderName.Length, 100));

                var extractDirectory = $@"{ConfigManager.SongDirectory}/{folderName} - {time}/";

                try
                {
                    // If we are importing map files, not zipped mapsets, we should never delete them
                    var deleteOriginalFile = false;
                    if (file.EndsWith(".qp"))
                    {
                        ExtractQuaverMapset(file, extractDirectory);
                        deleteOriginalFile = true;
                    }
                    else if (file.EndsWith(".qpl"))
                    {
                        selectedMap = ExtractQuaverPlaylist(file);
                        skipPlaylistReload = false;
                        deleteOriginalFile = true;
                    }
                    else if (file.EndsWith(".osz"))
                    {
                        Osu.ConvertOsz(file, extractDirectory);
                        deleteOriginalFile = true;
                    }
                    else if (file.EndsWith(".sm"))
                        Stepmania.ConvertFile(file, extractDirectory);
                    else if (file.EndsWith(".mc"))
                        Malody.ExtractFile(file, extractDirectory);
                    else if (file.EndsWith(".mcz"))
                    {
                        Malody.ExtractZip(file, extractDirectory);
                        deleteOriginalFile = true;
                    }

                    // Delete if the player has the option Delete Original File After Import on
                    // and the file to delete is a mapset, not a map file
                    // If we are importing a mapset downloaded from in-game downloader,
                    // We should delete it no matter what
                    if ((deleteOriginalFile && ConfigManager.DeleteOriginalFileAfterImport.Value)
                    || file.IsSubDirectoryOf(ConfigManager.DataDirectory.Value))
                        File.Delete(file);

                    if (!file.EndsWith(".qpl"))
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

            MapDatabaseCache.OrderAndSetMapsets(skipPlaylistReload);
            Queue.Clear();
            MapsetInfoRetriever.InfoUpdateEnabled = true;

            if (MapManager.Mapsets.Count == 0)
                return;

            // If specific map id was given, select that one.
            if (selectMapIdAfterImport != null)
            {
                var map = MapManager.FindMapFromOnlineId(selectMapIdAfterImport.Value);

                if (map != null)
                {
                    MapManager.Selected.Value = map;
                    return;
                }
            }

            var mapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Md5Checksum == selectedMap?.Md5Checksum));

            if (mapset == null)
            {
                mapset = MapManager.Mapsets.First();
                MapManager.SelectMapFromMapset(mapset);
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
            var options = new ExtractionOptions { ExtractFullPath = true, Overwrite = true };

            using (var archive = ArchiveFactory.Open(fileName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory)
                        continue;

                    try
                    {
                        entry.WriteToDirectory(extractPath, options);
                    }
                    catch (Exception e)
                    {
                        var path = entry.Key;
                        Logger.Warning($"Entry `{path}` failed to extract: {e}", LogType.Runtime);

                        if (Path.GetExtension(path) != ".qua" || Path.GetDirectoryName(path) != "")
                        {
                            // Can't try a different name for other files as they are referenced by name.
                            throw;
                        }

                        path = Path.Join(extractPath, CryptoHelper.StringToMd5(path) + ".qua");
                        Logger.Warning($"Trying with a different file name: {path}...", LogType.Runtime);

                        try
                        {
                            entry.WriteToFile(path, options);
                        }
                        catch (Exception e2)
                        {
                            Logger.Warning($"Entry `{path}` failed to extract: {e2}", LogType.Runtime);

                            // Oh well.
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Responsible for extracting the files from the .qpl
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extractPath"></param>
        private static Map ExtractQuaverPlaylist(string fileName)
        {
            var options = new ExtractionOptions { ExtractFullPath = true, Overwrite = true };

            var time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;
            var tempFolder = $@"{ConfigManager.TempDirectory}/{Path.GetFileNameWithoutExtension(fileName)} - {time}";

            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);

            Directory.CreateDirectory(tempFolder);

            // Extract files to temp directory
            using (var archive = ArchiveFactory.Open(fileName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                        entry.WriteToDirectory(tempFolder, options);
                }
            }

            // Import mapsets
            Map selectedMap = null;
            foreach (var path in Directory.GetFiles(tempFolder, "*.qp", SearchOption.AllDirectories))
            {
                // Basically recreate the import process
                var extractDirectory = $@"{ConfigManager.SongDirectory}/{Path.GetFileNameWithoutExtension(path)} - {time}/";

                ExtractQuaverMapset(path, extractDirectory);
                if (ConfigManager.DeleteOriginalFileAfterImport.Value || path.IsSubDirectoryOf(ConfigManager.DataDirectory.Value))
                    File.Delete(path);

                Logger.Important($"Successfully imported {path}", LogType.Runtime);

                selectedMap = InsertAndUpdateSelectedMap(extractDirectory);
            }

            // Find metadata file
            var metadata = Directory.GetFiles(tempFolder, "metadata.json").FirstOrDefault();
            if (metadata == null)
            {
                Logger.Log("metadata not found", LogLevel.Important, LogType.Runtime);
                return selectedMap;
            }

            // Create playlist
            dynamic json = JsonConvert.DeserializeObject<ExpandoObject>(File.ReadAllText(metadata));

            var onlineMapPoolId = (int)json.OnlineMapPoolId;
            if(onlineMapPoolId > -1)
                PlaylistManager.ImportPlaylist(onlineMapPoolId);
            else
            {
                var isNewPlaylist = false;
                var playlist = PlaylistManager.Playlists.Find(p => p.Name == json.Name && p.Description == json.Description && p.Creator == json.Creator);
                if(playlist == null)
                {
                    isNewPlaylist = true;
                    playlist = new Playlist()
                    {
                        Name = json.Name,
                        Description = json.Description,
                        Creator = json.Creator,
                        OnlineMapPoolId = onlineMapPoolId,
                        OnlineMapPoolCreatorId = (int)json.OnlineMapPoolCreatorId
                    };
                }
                foreach (var map in json.Maps)
                {
                    playlist.Maps.Add(new Map { Md5Checksum = map });
                }

                if(isNewPlaylist)
                    PlaylistManager.AddPlaylist(playlist);
                    
                playlist.Maps.ForEach(x => PlaylistManager.AddMapToPlaylist(playlist, x));
            }
            
            Directory.Delete(tempFolder, true);

            return selectedMap;
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
                    MapDatabaseCache.InsertMap(map);
                    MapsetInfoRetriever.Enqueue(map);
                    lastImported = map;
                }
            }
            catch (QuaVersionException e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, e.Message);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, "Error loading map, check runtime.log for details.");
            }

            return lastImported;
        }
    }
}
