/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
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
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using SQLite;
using Wobble;
using Wobble.Logging;
using GameMode = osu.Shared.GameMode;

namespace Quaver.Shared.Database.Maps
{
    public static class MapDatabaseCache
    {
        /// <summary>
        ///     The path of the local database
        /// </summary>
        private static readonly string DatabasePath = ConfigManager.GameDirectory + "/quaver.db";

        /// <summary>
        ///     Dictates if we currently have loaded maps from other games.
        /// </summary>
        public static bool LoadedMapsFromOtherGames { get; private set; }

        /// <summary>
        ///     Loads all of the maps in the database and groups them into mapsets to use
        ///     for gameplay
        /// </summary>
        public static void Load(bool fullSync)
        {
            if (fullSync)
            {
                if (File.Exists(DatabasePath))
                    File.Delete(DatabasePath);
            }

            CreateTable();

            // Fetch all of the .qua files inside of the song directory
            var quaFiles = Directory.GetFiles(ConfigManager.SongDirectory.Value, "*.qua", SearchOption.AllDirectories).ToList();
            Logger.Important($"Found {quaFiles.Count} .qua files inside the song directory", LogType.Runtime);

            SyncMissingOrUpdatedFiles(quaFiles);
            AddNonCachedFiles(quaFiles);

            OrderAndSetMapsets();
        }

        /// <summary>
        ///     Creates the `maps` database table.
        /// </summary>
        private static void CreateTable()
        {
            try
            {
                var conn = new SQLiteConnection(DatabasePath);
                conn.CreateTable<Map>();
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

            foreach (var map in maps)
            {
                var filePath = BackslashToForward($"{ConfigManager.SongDirectory.Value}/{map.Directory}/{map.Path}");

                // Check if the file actually exists.
                if (files.Any(x => BackslashToForward(x) == filePath))
                {
                    // Check if the file was updated. In this case, we check if the last write times are different
                    // BEFORE checking Md5 checksum of the file since it's faster to check if we even need to
                    // bother updating it.
                    if (map.LastFileWrite != File.GetLastWriteTimeUtc(filePath))
                    {
                        if (map.Md5Checksum == MapsetHelper.GetMd5Checksum(filePath))
                            continue;

                        var newMap = Map.FromQua(map.LoadQua(), filePath);
                        newMap.CalculateDifficulties();

                        newMap.Id = map.Id;
                        new SQLiteConnection(DatabasePath).Update(newMap);

                        Logger.Important($"Updated cached map: {newMap.Id}, as the file was updated.", LogType.Runtime);
                    }

                    continue;
                }

                // The file doesn't exist, so we can safely delete it from the cache.
                new SQLiteConnection(DatabasePath).Delete(map);
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

            foreach (var file in files)
            {
                if (maps.Any(x => BackslashToForward(file) == BackslashToForward($"{ConfigManager.SongDirectory.Value}/{x.Directory}/{x.Path}")))
                    continue;

                // Found map that isn't cached in the database yet.
                try
                {
                    var map = Map.FromQua(Qua.Parse(file), file);
                    map.CalculateDifficulties();
                    map.DifficultyProcessorVersion = DifficultyProcessorKeys.Version;
                    InsertMap(map, file);
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
        public static List<Map> FetchAll() => new SQLiteConnection(DatabasePath).Table<Map>().ToList();

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
        /// <param name="file"></param>
        public static void InsertMap(Map map, string file)
        {
            try
            {
                new SQLiteConnection(DatabasePath).Insert(map);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                File.Delete(file);
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
                new SQLiteConnection(DatabasePath).Update(map);
                Logger.Debug($"Updated map: {map.Md5Checksum} (#{map.Id}) in the cache", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Reads the osu!.db file defined in config and loads all of those maps into the cache.
        /// </summary>
        private static IEnumerable<Map> LoadOsuBeatmapDatabase()
        {
            try
            {
                var db = OsuDb.Read(ConfigManager.OsuDbPath.Value);
                MapManager.OsuSongsFolder = Path.GetDirectoryName(ConfigManager.OsuDbPath.Value) + "/Songs/";

                // Find all osu! maps that are 4K and 7K and order them by their difficulty value.
                var osuBeatmaps = db.Beatmaps.Where(x => x.GameMode == GameMode.Mania && ( x.CircleSize == 4 || x.CircleSize == 7 )).ToList();
                osuBeatmaps = osuBeatmaps.OrderBy(x => x.DiffStarRatingMania.ContainsKey(Mods.None) ? x.DiffStarRatingMania[Mods.None] : 0).ToList();

                var osuToQuaverMaps = new List<Map>();

                foreach (var map in osuBeatmaps)
                {
                    var newMap = new Map
                    {
                        Md5Checksum = map.BeatmapChecksum,
                        Directory = map.FolderName,
                        Path = map.BeatmapFileName,
                        Artist = map.Artist,
                        Title = map.Title,
                        MapSetId = -1,
                        MapId = -1,
                        DifficultyName = map.Version,
                        RankedStatus = RankedStatus.NotSubmitted,
                        Creator = map.Creator,
                        AudioPath = map.AudioFileName,
                        AudioPreviewTime = map.AudioPreviewTime,
                        Description = $"",
                        Source = map.SongSource,
                        Tags = map.SongTags,
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        Mode = map.CircleSize == 4 ? Quaver.API.Enums.GameMode.Keys4 : Quaver.API.Enums.GameMode.Keys7,
                        SongLength = map.TotalTime,
                        Game = MapGame.Osu,
                        BackgroundPath = "",
                    };

                    // Get the BPM of the osu! maps
                    if (map.TimingPoints != null)
                    {
                        try
                        {
                            newMap.Bpm = Math.Round(60000 / map.TimingPoints.Find(x => x.MsPerQuarter > 0).MsPerQuarter, 0);
                        }
                        catch (Exception e)
                        {
                            newMap.Bpm = 0;
                        }
                    }

                    osuToQuaverMaps.Add(newMap);
                }

                return osuToQuaverMaps;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);

                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen;

                if (screen != null)
                    NotificationManager.Show(NotificationLevel.Error, "Failed to load maps from other games. Is your db path correct in config?");

                return new List<Map>();
            }
        }

        /// <summary>
        ///     Fetches all maps, groups them into mapsets, sets them to allow them to be played.
        /// </summary>
        public static void OrderAndSetMapsets()
        {
            var maps = FetchAll();

            if (ConfigManager.AutoLoadOsuBeatmaps.Value)
            {
                maps = maps.Concat(LoadOsuBeatmapDatabase()).ToList();
                LoadedMapsFromOtherGames = true;
            }
            else
            {
                LoadedMapsFromOtherGames = false;
            }

            var mapsets = MapsetHelper.ConvertMapsToMapsets(maps);
            MapManager.Mapsets = MapsetHelper.OrderMapsByDifficulty(MapsetHelper.OrderMapsetsByArtist(mapsets));
        }
    }
}
