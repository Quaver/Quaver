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
using osu.Shared;
using osu_database_reader.BinaryFiles;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Graphics.Notifications;
using SharpCompress.Archives;
using SharpCompress.Common;
using SQLite;
using Wobble;
using Wobble.Logging;
using GameMode = osu.Shared.GameMode;

namespace Quaver.Shared.Database.Maps
{
    public static class MapDatabaseCache
    {
        /// <summary>
        ///     List of maps to force update after editing them.
        /// </summary>
        public static List<Map> MapsToUpdate { get; } = new List<Map>();

        /// <summary>
        ///     The names of the .qp files in the resources
        /// </summary>
        private static List<string> DefaultMapsetFiles { get; } = new List<string>()
        {
            "+TEK - Stars and Bunnies",
            "HyuN - CrossOver",
            "HyuN - Princess Of Winter",
            "Rabbit House - Have A Party Time!",
            "zetoban - Csikos Post",
            "zetoban - Umami Packed Mountaineering",
            "zetoban - Flowering Night",
            "Plum - Fantasy Collision",
            "Lollipop - vibing oustide the supermarket at 1am",
            "HyuN feat. Yuri - Disorder",
            "Rabbit House - Dead Rabbit And Witch's Blood"
        };

        /// <summary>
        ///     The default map that will be chosen if <see cref="ImportDefaultMapsets"/> is called
        ///     (HyuN - Princess of Winter [Beginner])
        /// </summary>
        private static string DefaultMapChecksum { get; } = "5004c3553cd29ccd3191e5c266f2f282";

        /// <summary>
        ///     Loads all of the maps in the database and groups them into mapsets to use
        ///     for gameplay
        /// </summary>
        public static void Load(bool fullSync)
        {
            CreateTable();

            // Fetch all of the .qua files inside of the song directory
            var quaFiles = Directory.GetFiles(ConfigManager.SongDirectory.Value, "*.qua", SearchOption.AllDirectories).ToList();
            Logger.Important($"Found {quaFiles.Count} .qua files inside the song directory", LogType.Runtime);

            if (fullSync)
            {
                SyncMissingOrUpdatedFiles(quaFiles);
                AddNonCachedFiles(quaFiles);
            }

            OrderAndSetMapsets();

            if (MapManager.Mapsets.Count == 0)
                ImportDefaultMapsets();
        }

        /// <summary>
        ///     Creates the `maps` database table.
        /// </summary>
        private static void CreateTable()
        {
            try
            {
                DatabaseManager.Connection.CreateTable<Map>();
                Logger.Important($"Map Database has been created", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Checks the maps in the database vs. the amount of .qua files on disk.
        ///     If there's a mismatch, it will add any missing ones
        /// </summary>
        private static void SyncMissingOrUpdatedFiles(IReadOnlyCollection<string> files)
        {
            var maps = FetchAll();

            var fileHashSet = files.ToHashSet();

            foreach (var map in maps)
            {
                var filePath = BackslashToForward($"{ConfigManager.SongDirectory.Value}/{map.Directory}/{map.Path}");

                // Check if the file actually exists.
                if (fileHashSet.Contains(BackslashToForward(filePath)))
                {
                    // Check if the file was updated. In this case, we check if the last write times are different
                    // BEFORE checking Md5 checksum of the file since it's faster to check if we even need to
                    // bother updating it.
                    if (map.LastFileWrite != File.GetLastWriteTimeUtc(filePath))
                    {
                        if (map.Md5Checksum == MapsetHelper.GetMd5Checksum(filePath))
                            continue;

                        Map newMap;

                        try
                        {
                            newMap = Map.FromQua(map.LoadQua(false), filePath);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, LogType.Runtime);
                            File.Delete(filePath);
                            DatabaseManager.Connection.Delete(map);
                            Logger.Important($"Removed {filePath} from the cache, as the file could not be parsed.", LogType.Runtime);
                            continue;
                        }

                        newMap.CalculateDifficulties();

                        newMap.Id = map.Id;
                        DatabaseManager.Connection.Update(newMap);

                        Logger.Important($"Updated cached map: {newMap.Id}, as the file was updated.", LogType.Runtime);
                    }

                    continue;
                }

                // The file doesn't exist, so we can safely delete it from the cache.
                DatabaseManager.Connection.Delete(map);
                Logger.Important($"Removed {filePath} from the cache, as the file no longer exists", LogType.Runtime);
            }
        }

        /// <summary>
        ///     Adds any new files that are currently not cached.
        ///     Used if the user adds a file to the folder.
        /// </summary>
        /// <param name="files"></param>
        private static void AddNonCachedFiles(List<string> files)
        {
            var maps = FetchAll();

            var hashset = new HashSet<string>();
            maps.ForEach(x => hashset.Add(BackslashToForward($"{ConfigManager.SongDirectory.Value}/{x.Directory}/{x.Path}")));

            foreach (var file in files)
            {
                if (hashset.Contains(BackslashToForward(file)))
                    continue;

                // Found map that isn't cached in the database yet.
                try
                {
                    var map = Map.FromQua(Qua.Parse(file, false), file);
                    InsertMap(map);

                    if (!QuaverSettingsDatabaseCache.OutdatedMaps.Contains(map))
                        QuaverSettingsDatabaseCache.OutdatedMaps.Add(map);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        ///     Responsible for fetching all the maps from the database and returning them.
        /// </summary>
        /// <returns></returns>
        public static List<Map> FetchAll() => DatabaseManager.Connection.Table<Map>().ToList();

        /// <summary>
        ///     Converts all backslash characters to forward slashes.
        ///     Used for paths
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static string BackslashToForward(string p) => p.Replace("\\", "/");

        /// <summary>
        ///     Inserts an individual map to the database.
        /// </summary>
        /// <param name="map"></param>
        public static int InsertMap(Map map)
        {
            try
            {
                DatabaseManager.Connection.Insert(map);

                return DatabaseManager.Connection.Get<Map>(x => x.Md5Checksum == map.Md5Checksum).Id;
            }
            catch (Exception e)
            {
                var existing = DatabaseManager.Connection.Find<Map>(x => x.Md5Checksum == map.Md5Checksum);
                if (existing == null)
                {
                    // Weird.
                    Logger.Error(e, LogType.Runtime);
                    return -1;
                }

                var newPath = Path.Combine(ConfigManager.SongDirectory.Value, map.Directory, map.Path);
                var existingPath = Path.Combine(ConfigManager.SongDirectory.Value, existing.Directory, existing.Path);

                if (existingPath != newPath)
                {
                    Logger.Warning($"Tried importing a duplicate of `{existingPath}` at `{newPath}`, deleting.", LogType.Runtime);
                    // Delete the duplicate file.
                    File.Delete(newPath);
                    return -1;
                }

                // Do not delete if the path matches.
                Logger.Warning($"Tried importing `{existingPath}` twice.", LogType.Runtime);
                return -1;
            }
        }

        /// <summary>
        ///     Updates an individual map in the database.
        /// </summary>
        /// <param name="map"></param>
        public static void UpdateMap(Map map)
        {
            try
            {
                DatabaseManager.Connection.Update(map);
                Logger.Debug($"Updated map: {map.Md5Checksum} (#{map.Id}) in the cache", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///    Removes an individual map in the database.
        /// </summary>
        /// <param name="map"></param>
        public static void RemoveMap(Map map)
        {
            try
            {
                DatabaseManager.Connection.Delete(map);
                Logger.Debug($"Deleted map: {map.Md5Checksum} (#{map.Id}) in the cache", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Map FindSet(int id)
        {
            try
            {
                return DatabaseManager.Connection.Find<Map>(x => x.MapSetId == id);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return null;
            }
        }

        /// <summary>
        ///     Fetches all maps, groups them into mapsets, sets them to allow them to be played.
        /// </summary>
        public static void OrderAndSetMapsets(bool skipPlaylistLoad = false)
        {
            OtherGameMapDatabaseCache.Initialize();

            var maps = FetchAll();

            if (ConfigManager.AutoLoadOsuBeatmaps.Value)
                maps = maps.Concat(OtherGameMapDatabaseCache.Load()).ToList();

            var mapsets = MapsetHelper.ConvertMapsToMapsets(maps);
            MapManager.Mapsets = MapsetHelper.OrderMapsByDifficulty(MapsetHelper.OrderMapsetsByArtist(mapsets));
            MapManager.RecentlyPlayed = new List<Map>();

            if(!skipPlaylistLoad)
                PlaylistManager.Load();

            // Schedule maps that don't have difficulty ratings to recalculate.
            // If forcing a full recalculation due to diff calc updates, then the difficulty processor version should just be bumped
            // instead of adding things here.
            foreach (var mapset in MapManager.Mapsets)
            {
                foreach (var map in mapset.Maps)
                {
                    // The difficulty calculator only calculates for maps with >= 2 hitobjects
                    if (map.RegularNoteCount + map.LongNoteCount >= 2)
                    {
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (map.Difficulty105X == 0f && !OtherGameMapDatabaseCache.MapsToCache[OtherGameCacheAction.Add].Contains(map))
                            OtherGameMapDatabaseCache.MapsToCache[OtherGameCacheAction.Update].Add(map);
                    }
                }
            }

            var outdated = FetchAll().FindAll(x => x.DifficultyProcessorVersion != DifficultyProcessorKeys.Version);
            OtherGameMapDatabaseCache.MapsToCache[OtherGameCacheAction.Update].AddRange(outdated);
        }

        /// <summary>
        /// </summary>
        public static void ForceUpdateMaps(bool createNewMaps = true)
        {
            for (var i = 0; i < MapsToUpdate.Count; i++)
            {
                try
                {
                    var path = $"{ConfigManager.SongDirectory}/{MapsToUpdate[i].Directory}/{MapsToUpdate[i].Path}";

                    if (!File.Exists(path))
                        continue;

                    Map map;

                    if (createNewMaps)
                        map = Map.FromQua(Qua.Parse(path, false), path);
                    else
                        map = MapsToUpdate[i];

                    map.CalculateDifficulties();
                    map.Id = MapsToUpdate[i].Id;
                    map.Directory = MapsToUpdate[i].Directory;
                    map.Mapset = MapsToUpdate[i].Mapset;

                    if (map.Id == 0)
                    {
                        map.Id = InsertMap(map);
                    }
                    else
                    {
                        UpdateMap(map);
                        PlaylistManager.UpdateMapInPlaylists(MapsToUpdate[i], map);
                    }

                    MapsToUpdate[i] = map;

                    if (createNewMaps)
                        MapManager.Selected.Value = map;
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            MapsToUpdate.Clear();

            if (createNewMaps)
            {
                OrderAndSetMapsets();
                PlaylistManager.Load();

                var selectedMapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Md5Checksum == MapManager.Selected.Value.Md5Checksum));
                MapManager.Selected.Value = selectedMapset.Maps.Find(x => x.Md5Checksum == MapManager.Selected.Value.Md5Checksum);
            }
        }

        /// <summary>
        ///     Extracts the .qp files of the default maps and imports them into the game
        /// </summary>
        private static void ImportDefaultMapsets()
        {
            foreach (var map in DefaultMapsetFiles)
            {
                var directory = $"{ConfigManager.SongDirectory.Value}/{map}";

                if (Directory.Exists(directory))
                    continue;

                Directory.CreateDirectory(directory);

                try
                {
                    var stream = GameBase.Game.Resources.GetStream($"Quaver.Resources/DefaultMaps/{map}.qp");

                    using (var archive = ArchiveFactory.Open(stream))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                                entry.WriteToDirectory(directory, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }

                    Logger.Important($"Successfully imported default map: {map}", LogType.Runtime);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to import default map: {map}", LogType.Runtime);
                    Logger.Error(e, LogType.Runtime);
                }
            }

            // Perform a full sync so that everything can be imported
            Load(true);

            var defaultMap = MapManager.FindMapFromMd5(DefaultMapChecksum);

            // Select the default map and track (HyuN - Princess of Winter)
            if (defaultMap != null)
            {
                MapManager.Selected.Value = defaultMap;
                AudioEngine.LoadCurrentTrack();
            }
        }
    }
}
