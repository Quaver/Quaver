using System;
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
using Quaver.StepMania;
using SQLite;

namespace Quaver.Database.Beatmaps
{
    internal static class BeatmapCache
    {
        /// <summary>
        ///     The name of the module - for logging purposes.
        /// </summary>
        private static readonly string Module = "[BEATMAP CACHE]";

        /// <summary>
        ///     The path of the local database
        /// </summary>
        private static readonly string DatabasePath = Configuration.GameDirectory + "/quaver.db";

        /// <summary>
        ///     Responsible for loading and setting our global beatmaps variable.
        /// </summary>
        public static async Task LoadAndSetBeatmaps()
        {
            GameBase.Mapsets = BeatmapHelper.OrderMapsByDifficulty(BeatmapHelper.OrderBeatmapsByArtist(await LoadBeatmapDatabaseAsync()));
            GameBase.VisibleMapsets = GameBase.Mapsets;
        }

        /// <summary>
        ///     Initializes and loads the beatmap database
        /// </summary>
        /// <returns></returns>
        private static async Task<List<Mapset>> LoadBeatmapDatabaseAsync()
        {
            try
            {
                // Create and sync the database.
                await CreateBeatmapTableAsync();
                await SyncBeatmapDatabaseAsync();

                // Fetch all the new beatmaps after syncing and return them.
                var beatmaps = await FetchAllBeatmaps();
                Logger.LogSuccess($"{beatmaps.Count} beatmaps have been successfully loaded.", LogType.Runtime);

                if (Configuration.AutoLoadOsuBeatmaps)
                    beatmaps = beatmaps.Concat(LoadBeatmapsFromOsuDb()).ToList();

                if (Configuration.AutoLoadEtternaCharts)
                    beatmaps = beatmaps.Concat(LoadBeatmapsFromEtternaCache()).ToList();

                var maps = BeatmapHelper.GroupBeatmapsByDirectory(beatmaps);
                Logger.LogSuccess($"Successfully loaded {beatmaps.Count} in {maps.Count} directories.", LogType.Runtime);

                return maps;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                File.Delete(DatabasePath);
                return await LoadBeatmapDatabaseAsync();
            }            
        }

        /// <summary>
        ///     Create the beatmap table. If there is an issue, we'll delete the database fully, and create it again.
        /// </summary>
        private static async Task CreateBeatmapTableAsync()
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DatabasePath);
                await conn.CreateTableAsync<Beatmap>();
                Logger.LogSuccess($"Beatmap Database has been created.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                throw;
            }
        }
        
        /// <summary>
        ///     Completely syncs our beatmap database. It will call our other helper methods, and makes sure
        ///     that maps are always up-to-date in the database.
        /// </summary>
        /// <returns></returns>
        private static async Task SyncBeatmapDatabaseAsync()
        {
            // Find all the.qua files in the directory.
            var Mapss = Directory.GetFiles(Configuration.SongDirectory, "*.qua", SearchOption.AllDirectories);
            Logger.LogInfo($"Found: {Mapss.Length} .qua files in the /songs/ directory.", LogType.Runtime);

            await CacheByFileCount(Mapss);

            // Remove any qua files from the list that don't actually exist.
            var MapsList = Mapss.ToList();
            MapsList.RemoveAll(x => !File.Exists(x));
            Mapss = MapsList.ToArray();

            Logger.LogImportant($"After removing missing .qua files, there are now {Mapss.Length}", LogType.Runtime);

            await CacheByMd5ChecksumAsync(Mapss);
            await RemoveMissingBeatmaps();
        }

        /// <summary>
        ///     Compares the amount of files on the file system vs. that of in the database.
        ///     It will add any of them that it finds aren't in there.
        /// </summary>
        /// <param name="Mapss"></param>
        /// <returns></returns>
        private static async Task CacheByFileCount(string[] Mapss)
        {
            var beatmapsInDb = await FetchAllBeatmaps();

            // We only want to add more beatmaps here if the counts don't add up.
            if (beatmapsInDb.Count == Mapss.Length)
                return;

            Logger.LogImportant($"Incorrect # of .qua files vs maps detected. {Mapss.Length} vs {beatmapsInDb.Count}", LogType.Runtime);

            // This'll store all the beatmaps we'll be adding into the database.
            var beatmapsToCache = new List<Beatmap>();

            // Parse & add each beatmap to the list of maps to cache if they aren't already in the database.
            foreach (var file in Mapss)
            {
                // Run a check to see if the beatmap path already exists in the database.
                if (beatmapsInDb.Any(beatmap => Configuration.SongDirectory.Replace("\\", "/") + "/" + beatmap.Directory + "/" + beatmap.Path.Replace("\\", "/") == file.Replace("\\", "/"))) continue;

                // Try to parse the file and check if it is a legitimate .qua file.
                var qua = Qua.Parse(file);
                if (!qua.IsValidQua)
                {
                    Logger.LogError($"Qua File {file} could not be parsed.", LogType.Runtime);
                    File.Delete(file);
                    continue;
                }

                // Convert the Qua into a Beatmap object and add it to our list of maps we want to cache.
                var newBeatmap = new Beatmap().ConvertQuaToBeatmap(qua, file);
                newBeatmap.Path = Path.GetFileName(newBeatmap.Path.Replace("\\", "/"));
                beatmapsToCache.Add(newBeatmap);
            }

            var finalList = await RemoveDuplicates(beatmapsToCache);

            // Add new beatmaps to the database.
            if (finalList.Count > 0)
                await InsertBeatmapsIntoDatabase(finalList);
        }

        /// <summary>
        ///     Compares the beatmaps MD5 checksums in that of the database and the file system, 
        ///     and updates/adds/removes them from the database.
        /// </summary>
        /// <returns></returns>
        private static async Task CacheByMd5ChecksumAsync(IEnumerable<string> Mapss)
        {
            // This'll hold all of the MD5 Checksums of the .qua files in the directory.
            // Since this is an updated list, we'll use these to check if they are in the database and unchanged.
            var fileChecksums = new List<string>();
            Mapss.ToList().ForEach(qua => fileChecksums.Add(BeatmapHelper.GetMd5Checksum(qua)));

            // Find all the beatmaps in the database
            var beatmapsInDb = await FetchAllBeatmaps();

            // Find all the mismatched beatmaps.
            var mismatchedBeatmaps = beatmapsInDb
                .Except(beatmapsInDb.Where(map => fileChecksums.Any(md5 => md5 == map.Md5Checksum)).ToList()).ToList();

            Logger.LogImportant($"Found: {mismatchedBeatmaps.Count} beatmaps with unmatched checksums", LogType.Runtime);

            if (mismatchedBeatmaps.Count > 0)
                await DeleteBeatmapsFromDatabase(mismatchedBeatmaps);

            // Stores the list of beatmaps that were successfully reprocessed and are ready to add into the DB.
            var reprocessedBeatmaps = new List<Beatmap>();
            foreach (var map in mismatchedBeatmaps)
            {
                if (!File.Exists(map.Path))
                    continue;

                // Parse the map again and add it to the list of maps to be added to the database.
                reprocessedBeatmaps.Add(new Beatmap().ConvertQuaToBeatmap(Qua.Parse(map.Path), map.Path));
            }

            var finalList = await RemoveDuplicates(reprocessedBeatmaps);

            // Add new beatmaps to the database.
            if (finalList.Count > 0)
                await InsertBeatmapsIntoDatabase(finalList);
        }

        /// <summary>
        ///     Takes a look at all the beatmaps that are in the db and removes any of the missing ones.
        /// </summary>
        /// <returns></returns>
        private static async Task RemoveMissingBeatmaps()
        {
            var beatmaps = await FetchAllBeatmaps();
            
            // Stores the maps we need to delete
            var mapsToDelete = new List<Beatmap>();
            
            foreach (var map in beatmaps)
            {
                var mapPath = $"{Configuration.SongDirectory}/{map.Directory}/{map.Path}".Replace("\\", "/");

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
            await DeleteBeatmapsFromDatabase(mapsToDelete);
        }

        /// <summary>
        ///     Responsible for fetching all the beatmaps from the database and returning them.
        /// </summary>
        /// <returns></returns>
        private static async Task<List<Beatmap>> FetchAllBeatmaps()
        {
            return await new SQLiteAsyncConnection(DatabasePath).Table<Beatmap>().ToListAsync().ContinueWith(t => t.Result);
        }

        /// <summary>
        ///     Responsible for taking a list of beatmaps from the database and adding them to it.
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        private static async Task InsertBeatmapsIntoDatabase(List<Beatmap> beatmaps)
        {
            try
            {
                // Remove all the duplicate MD5 beatmaps in the list before inserting.
                RemoveDuplicatesInList(beatmaps);

                if (beatmaps.Count > 0)
                    await new SQLiteAsyncConnection(DatabasePath).InsertAllAsync(beatmaps)
                        .ContinueWith(t => Logger.LogSuccess($"Successfully added {beatmaps.Count} beatmaps to the database", LogType.Runtime));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Responsible for removing a list of beatmaps from the database.
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        private static async Task DeleteBeatmapsFromDatabase(List<Beatmap> beatmaps)
        {
            try
            {
                foreach (var map in beatmaps)
                    await new SQLiteAsyncConnection(DatabasePath).DeleteAsync(map);

                Logger.LogSuccess($"Successfully deleted {beatmaps.Count} beatmaps from the database.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Removes duplicate maps from a list of beatmaps.
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        private static async Task<List<Beatmap>> RemoveDuplicates(List<Beatmap> beatmaps)
        {
            // Add a check for duplicate maps being inserted into the database.
            // we do this by MD5 Checksum. Also delete them if that's the case.
            var mapsInDb = await FetchAllBeatmaps();

            // Create the final list of beatmaps to be added.
            var finalMaps = new List<Beatmap>();

            foreach (var map in beatmaps)
            {
                if (mapsInDb.Any(x => x.Md5Checksum == map.Md5Checksum))
                {
                    File.Delete(Configuration.SongDirectory + "/" + map.Directory + "/" + map.Path);
                    continue;
                }

                finalMaps.Add(map);
            }

            return finalMaps;
        }

        /// <summary>
        ///     Finds and removes duplicates in the list of beatmaps itself. This is usually called before inserting,
        ///     to make sure we're not adding the same map twice.
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        private static void RemoveDuplicatesInList(List<Beatmap> beatmaps)
        {
            var duplicateInsertEntries = beatmaps
                .GroupBy(x => x.Md5Checksum)
                .Select(g => g.Count() > 1 ? g.First() : null)
                .ToList();

            // Delete all the duplicate entries
            foreach (var map in duplicateInsertEntries)
            {
                try
                {
                    beatmaps.Remove(map);

                    // Continue to the next map if the current one is indeed null.
                    if (map == null)
                        continue;

                    // Delete the file if its actually a duplicate.
                    File.Delete($"{Configuration.SongDirectory}/{map.Directory}/{map.Path}");
                    Logger.LogImportant($"Removed duplicate map: {Configuration.SongDirectory}/{map.Directory}/{map.Path}", LogType.Runtime);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        ///     Updates a beatmap in the database
        /// </summary>
        /// <param name="beatmap"></param>
        /// <returns></returns>
        internal static async Task UpdateBeatmap(Beatmap beatmap)
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DatabasePath);
                await conn.ExecuteAsync("UPDATE Beatmap SET HighestRank = ?, LastPlayed = ? Where Id = ?", beatmap.HighestRank, beatmap.LastPlayed, beatmap.Id);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Method to update local offset in beatmap
        /// </summary>
        /// <param name="beatmap"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal static async Task UpdateLocalOffset(Beatmap beatmap, int offset)
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DatabasePath);
                await conn.ExecuteAsync("UPDATE Beatmap SET LocalOffset = ? Where Id = ?", offset, beatmap.Id);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Loads all osu! beatmaps from the osu!.db file
        /// </summary>
        private static List<Beatmap> LoadBeatmapsFromOsuDb()
        {
            try
            {
                var db = OsuDb.Read(Configuration.OsuDbPath);
                GameBase.OsuSongsFolder = Path.GetDirectoryName(Configuration.OsuDbPath) + "/Songs/";

                var beatmaps = db.Beatmaps.Where(x => x.GameMode == GameMode.Mania && (x.CircleSize == 4 || x.CircleSize == 7)).ToList();

                var maps = new List<Beatmap>();

                foreach (var beatmap in beatmaps)
                {
                    var newMap = new Beatmap
                    {
                        Md5Checksum = beatmap.BeatmapChecksum,
                        Directory = beatmap.FolderName,
                        Path = beatmap.BeatmapFileName,
                        Artist = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(beatmap.Artist)),
                        Title = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(beatmap.Title)),
                        MapSetId = -1,
                        MapId = -1,
                        DifficultyName = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(beatmap.Version)),
                        RankedStatus = 0,
                        DifficultyRating = 0,
                        Creator = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(beatmap.Creator)),
                        AudioPath = beatmap.AudioFileName,
                        AudioPreviewTime = beatmap.AudioPreviewTime,
                        Description = $"This map is a Quaver converted version of {Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(beatmap.Creator))}'s map",
                        Source = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(beatmap.SongSource)),
                        Tags = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(beatmap.SongTags)),
                        Mode = (beatmap.CircleSize == 4) ? GameModes.Keys4 : GameModes.Keys7,
                        SongLength = beatmap.TotalTime,
                        Game = BeatmapGame.Osu,
                        BackgroundPath = "",      
                    };

                    // Get the BPM of the osu! maps
                    if (beatmap.TimingPoints != null)
                    {
                        try
                        {
                            newMap.Bpm = Math.Round(60000 / beatmap.TimingPoints.Find(x => x.MsPerQuarter > 0).MsPerQuarter, 0);
                        }
                        catch (Exception e)
                        {
                            newMap.Bpm = 0;
                        }
                    }

                    maps.Add(newMap);
                }

                Logger.LogSuccess($"Finished loading: {beatmaps.Count} osu!mania beatmaps", LogType.Runtime);
                return maps;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                return new List<Beatmap>();
            }
        }

        /// <summary>
        ///     Loads charts from Etterna and places them on the cache.
        /// </summary>
        /// <returns></returns>
        private static List<Beatmap> LoadBeatmapsFromEtternaCache()
        {
            var maps = new List<Beatmap>();

            try
            {
                var files = Directory.GetFiles(Configuration.EtternaCacheFolderPath);

                if (files.Length == 0)
                    return maps;

                // Should give us the etterna base folder
                GameBase.EtternaFolder = Path.GetFullPath(Path.Combine(Configuration.EtternaCacheFolderPath, @"..\..\"));

                // Read all the files in them
                foreach (var cacheFile in files)
                {
                    try
                    {
                        var etternaFile = Etterna.ReadCacheFile(cacheFile);

                        foreach (var chart in etternaFile.ChartData)
                        {
                            maps.Add(new Beatmap()
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
                                Game = BeatmapGame.Etterna,
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
