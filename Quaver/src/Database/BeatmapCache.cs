﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Commands;
using Quaver.Database.Beatmaps;
using Quaver.Config;
using Quaver.Logging;
using SQLite;

namespace Quaver.Database
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
            GameBase.Mapsets = BeatmapUtils.OrderBeatmapsByArtist(await LoadBeatmapDatabaseAsync());
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
                Logger.Log($"{beatmaps.Count} beatmaps have been successfully loaded.", Color.Cyan);

                var maps = BeatmapUtils.GroupBeatmapsByDirectory(beatmaps);
                Logger.Log($"Successfully loaded {beatmaps.Count} in {maps.Count} directories.", Color.Cyan);

                return maps;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Cyan);
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
                Logger.Log($"Beatmap Database has been created.", Color.Cyan);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Cyan);
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
            Logger.Log($"Found: {Mapss.Length} .qua files in the /songs/ directory.", Color.Cyan);


            await CacheByFileCount(Mapss);

            // Remove any qua files from the list that don't actually exist.
            var MapsList = Mapss.ToList();
            MapsList.RemoveAll(x => !File.Exists(x));
            Mapss = MapsList.ToArray();
            Logger.Log($"After removing missing .qua files, there are now {Mapss.Length}", Color.Cyan);

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

            Logger.Log($"Incorrect # of .qua files vs maps detected. {Mapss.Length} vs {beatmapsInDb.Count}", Color.Red);

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
                    Logger.Log($"Error: Qua File {file} could not be parsed.", Color.Red);
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
            Mapss.ToList().ForEach(qua => fileChecksums.Add(BeatmapUtils.GetMd5Checksum(qua)));

            // Find all the beatmaps in the database
            var beatmapsInDb = await FetchAllBeatmaps();

            // Find all the mismatched beatmaps.
            var mismatchedBeatmaps = beatmapsInDb
                .Except(beatmapsInDb.Where(map => fileChecksums.Any(md5 => md5 == map.Md5Checksum)).ToList()).ToList();

            Logger.Log($"Found: {mismatchedBeatmaps.Count} beatmaps with unmatched checksums", Color.Cyan);
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
                    Logger.Log(e.Message, Color.Red);
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
                        .ContinueWith(t => Logger.Log($"Successfully added {beatmaps.Count} beatmaps to the database", Color.Cyan));
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
                {
                    await new SQLiteAsyncConnection(DatabasePath).DeleteAsync(map);
                }

                Logger.Log($"Successfully deleted {beatmaps.Count} beatmaps from the database.", Color.Cyan);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red);
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
                    Logger.Log($"Removed duplicate map: {Configuration.SongDirectory}/{map.Directory}/{map.Path}", Color.Pink);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message, Color.Cyan);
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
                Logger.Log(e.Message, LogColors.GameError);
                throw;
            }
        }
    }
}
