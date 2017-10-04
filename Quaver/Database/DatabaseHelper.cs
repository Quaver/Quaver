using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.QuaFile;
using SQLite;

namespace Quaver.Database
{
    internal static class DatabaseHelper
    {
        /// <summary>
        ///     Holds the path of the SQLite database
        /// </summary>
        internal static readonly string databasePath = Configuration.GameDirectory + "/quaver.db";

        /// <summary>
        ///     Initializes the beatmap database & create a table named Beatmaps with the Beatmap class properties.
        /// </summary>
        internal static async Task InitializeBeatmapDatabaseAsync()
        {
            var conn = new SQLiteAsyncConnection(databasePath);
            await conn.CreateTableAsync<Beatmap>();
            await SyncBeatmapDatabaseAsync();
            Console.WriteLine("[DATABASE HELPER] Beatmap synchronization completed!");
        }

        /// <summary>
        ///     Syncs the beatmap database. Makes sure that only playable beatmaps are in the database, and deletes
        ///     all corrupt ones.
        /// </summary>
        internal static async Task SyncBeatmapDatabaseAsync()
        {
            // Find all .qua files in the songs folder.
            var files = Directory.GetFiles(Configuration.SongDirectory, "*.qua", SearchOption.AllDirectories);
            Console.WriteLine($"[DATABASE HELPER] Found {files.Length} .qua files in the /Songs/ Directory!");

            // Find all of the beatmaps currently in the database, and check if the amount matches the # of found files.
            var conn = new SQLiteAsyncConnection(databasePath);
            var query = conn.Table<Beatmap>();

            await query.ToListAsync().ContinueWith(async t =>
            {
                var foundMaps = t.Result;
                Console.WriteLine($"[DATABASE HELPER] Found {foundMaps.Count} beatmaps in the database!");

                if (foundMaps.Count != files.Length)
                {
                    Console.WriteLine(
                        $"[DATABASE HELPER] Incorrect number of .qua files vs maps detected. {files.Length} vs {foundMaps.Count}!");

                    // This'll store the list of beatmaps that we'll be adding to the database.
                    var beatmapsToCache = new List<Beatmap>();

                    // For every .qua file in the directory, we'll want to parse them and add it to the database.
                    foreach (var quaFile in files)
                    {
                        // First run a check if the map exists in the database already
                        // To avoid potential path issues, we change all backslashes to forward slashes.
                        var inDb = foundMaps.Any(map => map.Path.Replace("\\", "/") == quaFile.Replace("\\", "/"));
                        if (inDb) continue;

                        // Try to parse the beatmap, convert it to a beatmap object and add it to the database
                        var parsedQua = new Qua(quaFile);

                        // If the file couldn't be parsed correctly, we want to display an error and delete the file.
                        if (!parsedQua.IsValidQua)
                        {
                            Console.WriteLine(
                                $"[DATABASE HELPER] Error: .Qua could not successfully be parsed: {quaFile}!");
                            File.Delete(quaFile);
                            continue;
                        }

                        // Create a new beatmap object and convert the parsed qua to it.
                        var newMapToCache = new Beatmap().ConvertQuaToBeatmap(parsedQua, quaFile);
                        newMapToCache.Path = newMapToCache.Path.Replace("\\", "/");
                        beatmapsToCache.Add(newMapToCache);
                    }

                    // Add all the beatmaps to the database
                    if (beatmapsToCache.Count > 0)
                        await AddBeatmapsToDatabase(beatmapsToCache);
                }
            });

            // Now that the file count is synced, let's remove the records that don't have attached beatmaps.
            await SyncMissingQuaFromDatabase(files);
        }

        /// <summary>
        ///     Adds a list of beatmaps to the database
        /// </summary>
        /// <returns></returns>
        private static async Task AddBeatmapsToDatabase(List<Beatmap> beatmaps)
        {
            Console.WriteLine($"[DATABASE HELPER] Adding {beatmaps.Count} beatmaps to the database...");
            var conn = new SQLiteAsyncConnection(databasePath);

            var watch = Stopwatch.StartNew();

            await conn.InsertAllAsync(beatmaps).ContinueWith(t =>
                Console.WriteLine($"[DATABASE HELPER] Successfully added {beatmaps.Count} beatmaps to the database."));

            watch.Stop();
            Console.WriteLine($"[DATABASE HELPER] Beatmap importing took {watch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        ///     Responsible for removing any beatmaps from the database that don't have valid qua files.
        /// </summary>
        /// <returns></returns>
        private static async Task SyncMissingQuaFromDatabase(string[] files)
        {
            var conn = new SQLiteAsyncConnection(databasePath);
            var query = conn.Table<Beatmap>();

            await query.ToListAsync().ContinueWith(t =>
            {
                var foundMaps = t.Result;

                var mapsToRemove = new List<Beatmap>();

                foreach (var map in foundMaps)
                {
                    var matchedQua = files.Any(file => file.Replace("\\", "/") == map.Path);
                    if (!matchedQua) mapsToRemove.Add(map);
                }

                // Now that we have all of the maps that don't have .qua files, let's remove them from the database.
                if (mapsToRemove.Count >0)
                    RemoveBeatmapsFromDatabase(mapsToRemove);
            });
        }

        /// <summary>
        ///     Removes a list of beatmaps from the database
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        private static void RemoveBeatmapsFromDatabase(List<Beatmap> beatmaps)
        {
            Console.WriteLine($"[DATABASE HELPER] Removing {beatmaps.Count} beatmaps from the database...");

            var conn = new SQLiteConnection(databasePath);

            foreach (var map in beatmaps)
                try
                {
                    conn.Execute($"DELETE from beatmap WHERE path=\"{map.Path}\"");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
        }
    }
}