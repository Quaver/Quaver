﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Shared;
using osu_database_reader.BinaryFiles;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Main;
using Quaver.StepMania;
using SQLite;

namespace Quaver.Database.Maps
{
    internal static class MapCache
    {
        /// <summary>
        ///     The name of the module - for logging purposes.
        /// </summary>
        private static readonly string Module = "[MAP CACHE]";

        /// <summary>
        ///     The path of the local database
        /// </summary>
        private static readonly string DatabasePath = ConfigManager.GameDirectory + "/quaver.db";

        /// <summary>
        ///     Responsible for loading and setting our global maps variable.
        /// </summary>
        public static async Task LoadAndSetMapsets()
        {
            GameBase.Mapsets = MapsetHelper.OrderMapsetsByDifficulty(MapsetHelper.OrderMapsetsByArtist(await LoadMapDatabaseAsync()));
            GameBase.VisibleMapsets = GameBase.Mapsets;
        }

        /// <summary>
        ///     Initializes and loads the map database
        /// </summary>
        /// <returns></returns>
        private static async Task<List<Mapset>> LoadMapDatabaseAsync()
        {
            try
            {
                // Create and sync the database.
                await CreateMapTableAsync();
                await SyncMapDatabaseAsync();

                // Fetch all the new maps after syncing and return them.
                var maps = await FetchAllMaps();
                Logger.LogSuccess($"{maps.Count} maps have been successfully loaded.", LogType.Runtime);

                if (ConfigManager.AutoLoadOsuBeatmaps)
                    maps = maps.Concat(LoadMapsFromOsuDb()).ToList();

                if (ConfigManager.AutoLoadEtternaCharts)
                    maps = maps.Concat(LoadMapsFromEtternaCache()).ToList();

                var mapsets = MapsetHelper.ConvertMapsToMapsets(maps);
                Logger.LogSuccess($"Successfully loaded {maps.Count} in {mapsets.Count} directories.", LogType.Runtime);

                return mapsets;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                File.Delete(DatabasePath);
                return await LoadMapDatabaseAsync();
            }            
        }

        /// <summary>
        ///     Create the map table. If there is an issue, we'll delete the database fully, and create it again.
        /// </summary>
        private static async Task CreateMapTableAsync()
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DatabasePath);
                await conn.CreateTableAsync<Map>();
                Logger.LogSuccess($"Map Database has been created.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                throw;
            }
        }
        
        /// <summary>
        ///     Completely syncs our map database. It will call our other helper methods, and makes sure
        ///     that maps are always up-to-date in the database.
        /// </summary>
        /// <returns></returns>
        private static async Task SyncMapDatabaseAsync()
        {
            // Find all the.qua files in the directory.
            var Mapss = Directory.GetFiles(ConfigManager.SongDirectory, "*.qua", SearchOption.AllDirectories);
            Logger.LogInfo($"Found: {Mapss.Length} .qua files in the /songs/ directory.", LogType.Runtime);

            await CacheByFileCount(Mapss);

            // Remove any qua files from the list that don't actually exist.
            var MapsList = Mapss.ToList();
            MapsList.RemoveAll(x => !File.Exists(x));
            Mapss = MapsList.ToArray();

            Logger.LogImportant($"After removing missing .qua files, there are now {Mapss.Length}", LogType.Runtime);

            await CacheByMd5ChecksumAsync(Mapss);
            await RemoveMissingMaps();
        }

        /// <summary>
        ///     Compares the amount of files on the file system vs. that of in the database.
        ///     It will add any of them that it finds aren't in there.
        /// </summary>
        /// <param name="Mapss"></param>
        /// <returns></returns>
        private static async Task CacheByFileCount(string[] Mapss)
        {
            var mapInDb = await FetchAllMaps();

            // We only want to add more maps here if the counts don't add up.
            if (mapInDb.Count == Mapss.Length)
                return;

            Logger.LogImportant($"Incorrect # of .qua files vs maps detected. {Mapss.Length} vs {mapInDb.Count}", LogType.Runtime);

            // This'll store all the maps we'll be adding into the database.
            var mapsToCache = new List<Map>();

            // Parse & add each map to the list of maps to cache if they aren't already in the database.
            foreach (var file in Mapss)
            {
                // Run a check to see if the map path already exists in the database.
                if (mapInDb.Any(map => ConfigManager.SongDirectory.Replace("\\", "/") + "/" + map.Directory + "/" + map.Path.Replace("\\", "/") == file.Replace("\\", "/"))) continue;

                // Try to parse the file and check if it is a legitimate .qua file.
                var qua = Qua.Parse(file);
                if (!qua.IsValidQua)
                {
                    Logger.LogError($"Qua File {file} could not be parsed.", LogType.Runtime);
                    File.Delete(file);
                    continue;
                }

                // Convert the Qua into a Map object and add it to our list of maps we want to cache.
                var newMap = new Map().ConvertQuaToMap(qua, file);
                newMap.Path = Path.GetFileName(newMap.Path.Replace("\\", "/"));
                mapsToCache.Add(newMap);
            }

            var finalList = await RemoveDuplicates(mapsToCache);

            // Add new maps to the database.
            if (finalList.Count > 0)
                await InsertMapsIntoDatabase(finalList);
        }

        /// <summary>
        ///     Compares the maps MD5 checksums in that of the database and the file system, 
        ///     and updates/adds/removes them from the database.
        /// </summary>
        /// <returns></returns>
        private static async Task CacheByMd5ChecksumAsync(IEnumerable<string> Mapss)
        {
            // This'll hold all of the MD5 Checksums of the .qua files in the directory.
            // Since this is an updated list, we'll use these to check if they are in the database and unchanged.
            var fileChecksums = new List<string>();
            Mapss.ToList().ForEach(qua => fileChecksums.Add(MapsetHelper.GetMd5Checksum(qua)));

            // Find all the maps in the database
            var mapInDb = await FetchAllMaps();

            // Find all the mismatched maps.
            var mismatchedMaps = mapInDb
                .Except(mapInDb.Where(map => fileChecksums.Any(md5 => md5 == map.Md5Checksum)).ToList()).ToList();

            Logger.LogImportant($"Found: {mismatchedMaps.Count} maps with unmatched checksums", LogType.Runtime);

            if (mismatchedMaps.Count > 0)
                await DeleteMapsFromDatabase(mismatchedMaps);

            // Stores the list of maps that were successfully reprocessed and are ready to add into the DB.
            var reprocessedMaps = new List<Map>();
            foreach (var map in mismatchedMaps)
            {
                if (!File.Exists(map.Path))
                    continue;

                // Parse the map again and add it to the list of maps to be added to the database.
                reprocessedMaps.Add(new Map().ConvertQuaToMap(Qua.Parse(map.Path), map.Path));
            }

            var finalList = await RemoveDuplicates(reprocessedMaps);

            // Add new maps to the database.
            if (finalList.Count > 0)
                await InsertMapsIntoDatabase(finalList);
        }

        /// <summary>
        ///     Takes a look at all the maps that are in the db and removes any of the missing ones.
        /// </summary>
        /// <returns></returns>
        private static async Task RemoveMissingMaps()
        {
            var maps = await FetchAllMaps();
            
            // Stores the maps we need to delete
            var mapsToDelete = new List<Map>();
            
            foreach (var map in maps)
            {
                var mapPath = $"{ConfigManager.SongDirectory}/{map.Directory}/{map.Path}".Replace("\\", "/");

                if (File.Exists(mapPath))
                    continue;

                try
                {
                    File.Delete(mapPath);
                    mapsToDelete.Add(map);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, LogType.Runtime);
                }
            }
            await DeleteMapsFromDatabase(mapsToDelete);
        }

        /// <summary>
        ///     Responsible for fetching all the maps from the database and returning them.
        /// </summary>
        /// <returns></returns>
        private static async Task<List<Map>> FetchAllMaps()
        {
            return await new SQLiteAsyncConnection(DatabasePath).Table<Map>().ToListAsync().ContinueWith(t => t.Result);
        }

        /// <summary>
        ///     Responsible for taking a list of maps from the database and adding them to it.
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        private static async Task InsertMapsIntoDatabase(List<Map> maps)
        {
            try
            {
                // Remove all the duplicate MD5 maps in the list before inserting.
                RemoveDuplicatesInList(maps);

                if (maps.Count > 0)
                    await new SQLiteAsyncConnection(DatabasePath).InsertAllAsync(maps)
                        .ContinueWith(t => Logger.LogSuccess($"Successfully added {maps.Count} maps to the database", LogType.Runtime));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Responsible for removing a list of maps from the database.
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        private static async Task DeleteMapsFromDatabase(List<Map> maps)
        {
            try
            {
                foreach (var map in maps)
                    await new SQLiteAsyncConnection(DatabasePath).DeleteAsync(map);

                Logger.LogSuccess($"Successfully deleted {maps.Count} maps from the database.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Removes duplicate maps from a list of maps.
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        private static async Task<List<Map>> RemoveDuplicates(List<Map> maps)
        {
            // Add a check for duplicate maps being inserted into the database.
            // we do this by MD5 Checksum. Also delete them if that's the case.
            var mapsInDb = await FetchAllMaps();

            // Create the final list of maps to be added.
            var finalMaps = new List<Map>();

            foreach (var map in maps)
            {
                if (mapsInDb.Any(x => x.Md5Checksum == map.Md5Checksum))
                {
                    File.Delete(ConfigManager.SongDirectory + "/" + map.Directory + "/" + map.Path);
                    continue;
                }

                finalMaps.Add(map);
            }

            return finalMaps;
        }

        /// <summary>
        ///     Finds and removes duplicates in the list of maps itself. This is usually called before inserting,
        ///     to make sure we're not adding the same map twice.
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        private static void RemoveDuplicatesInList(List<Map> maps)
        {
            var duplicateInsertEntries = maps
                .GroupBy(x => x.Md5Checksum)
                .Select(g => g.Count() > 1 ? g.First() : null)
                .ToList();

            // Delete all the duplicate entries
            foreach (var map in duplicateInsertEntries)
            {
                try
                {
                    maps.Remove(map);

                    // Continue to the next map if the current one is indeed null.
                    if (map == null)
                        continue;

                    // Delete the file if its actually a duplicate.
                    File.Delete($"{ConfigManager.SongDirectory}/{map.Directory}/{map.Path}");
                    Logger.LogImportant($"Removed duplicate map: {ConfigManager.SongDirectory}/{map.Directory}/{map.Path}", LogType.Runtime);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        ///     Updates a map in the database
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        internal static async Task UpdateMap(Map map)
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DatabasePath);
                await conn.ExecuteAsync("UPDATE Map SET HighestRank = ?, LastPlayed = ? Where Id = ?", map.HighestRank, map.LastPlayed, map.Id);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Method to update local offset in map
        /// </summary>
        /// <param name="map"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal static async Task UpdateLocalOffset(Map map, int offset)
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DatabasePath);
                await conn.ExecuteAsync("UPDATE Map SET LocalOffset = ? Where Id = ?", offset, map.Id);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Loads all osu! maps from the osu!.db file
        /// </summary>
        private static List<Map> LoadMapsFromOsuDb()
        {
            try
            {
                var db = OsuDb.Read(ConfigManager.OsuDbPath);
                GameBase.OsuSongsFolder = Path.GetDirectoryName(ConfigManager.OsuDbPath) + "/Songs/";

                var mapsFound = db.Beatmaps.Where(x => x.GameMode == GameMode.Mania && (x.CircleSize == 4 || x.CircleSize == 7)).ToList();

                var maps = new List<Map>();

                foreach (var map in mapsFound)
                {
                    var newMap = new Map
                    {
                        Md5Checksum = map.BeatmapChecksum,
                        Directory = map.FolderName,
                        Path = map.BeatmapFileName,
                        Artist = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(map.Artist)),
                        Title = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(map.Title)),
                        MapSetId = -1,
                        MapId = -1,
                        DifficultyName = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(map.Version)),
                        RankedStatus = 0,
                        DifficultyRating = 0,
                        Creator = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(map.Creator)),
                        AudioPath = map.AudioFileName,
                        AudioPreviewTime = map.AudioPreviewTime,
                        Description = $"This map is a Quaver converted version of {Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(map.Creator))}'s map",
                        Source = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(map.SongSource)),
                        Tags = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(map.SongTags)),
                        Mode = (map.CircleSize == 4) ? GameModes.Keys4 : GameModes.Keys7,
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

                    maps.Add(newMap);
                }

                Logger.LogSuccess($"Finished loading: {maps.Count} osu!mania maps", LogType.Runtime);
                return maps;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                return new List<Map>();
            }
        }

        /// <summary>
        ///     Loads charts from Etterna and places them on the cache.
        /// </summary>
        /// <returns></returns>
        private static List<Map> LoadMapsFromEtternaCache()
        {
            var maps = new List<Map>();

            try
            {
                var files = Directory.GetFiles(ConfigManager.EtternaCacheFolderPath);

                if (files.Length == 0)
                    return maps;

                // Should give us the etterna base folder
                GameBase.EtternaFolder = Path.GetFullPath(Path.Combine(ConfigManager.EtternaCacheFolderPath, @"..\..\"));

                // Read all the files in them
                foreach (var cacheFile in files)
                {
                    try
                    {
                        var etternaFile = Etterna.ReadCacheFile(cacheFile);

                        foreach (var chart in etternaFile.ChartData)
                        {
                            maps.Add(new Map()
                            {
                                Md5Checksum = chart.ChartKey,
                                Directory = Path.GetDirectoryName(etternaFile.SongFileName),
                                Path = Path.GetFileName(etternaFile.SongFileName),
                                Artist = etternaFile.Artist,
                                Title = etternaFile.Title,
                                MapSetId = -1,
                                MapId = -1,
                                DifficultyName = chart.Difficulty,
                                RankedStatus = 0,
                                DifficultyRating = 0,
                                Creator = etternaFile.Credit,
                                AudioPath = etternaFile.Music,
                                AudioPreviewTime = (int)etternaFile.SampleStart,
                                Description = $"This map is a StepMania converted version of {etternaFile.Credit}'s chart",
                                Source = "StepMania",
                                Tags = "StepMania",
                                Mode = GameModes.Keys4,
                                SongLength = 0,
                                Game = MapGame.Etterna,
                                BackgroundPath = etternaFile.Background,
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        // Couldn't parse, so just continue
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }

            return maps;
        }
    }
}
