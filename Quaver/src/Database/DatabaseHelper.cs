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
        private static readonly string DatabasePath = Configuration.GameDirectory + "/quaver.db";

        /// <summary>
        ///     The name of the module we'll be using for all logging purposes.
        /// </summary>
        private static readonly string module = "[DATABASE HELPER]";

        /// <summary>
        ///     Initializes the beatmap database & create a table named Beatmaps with the Beatmap class properties.
        /// </summary>
        internal static async Task<List<Beatmap>> InitializeBeatmapDatabaseAsync()
        {
            var conn = new SQLiteAsyncConnection(DatabasePath);
            await conn.CreateTableAsync<Beatmap>();
            await SyncBeatmapDatabaseAsync();

            // Get a list of all the beatmaps, once synced.
            var beatmaps = await GetAllBeatmaps();
            Console.WriteLine($"[DATABASE HELPER] {beatmaps.Count} beatmaps were successfully loaded.");
            return beatmaps;
        }

        /// <summary>
        ///     Asynchronously gets all the beatmaps from the database.
        /// </summary>
        /// <returns></returns>
        internal static async Task<List<Beatmap>> GetAllBeatmaps()
        {
            var Beatmaps = new List<Beatmap>();

            var conn = new SQLiteAsyncConnection(DatabasePath);
            var query = conn.Table<Beatmap>();

            await query.ToListAsync().ContinueWith(t => Beatmaps = t.Result);

            return Beatmaps;
        }

        /// <summary>
        ///     Syncs the beatmap database. Makes sure that only playable beatmaps are in the database, and deletes
        ///     all corrupt ones.
        /// </summary>
        internal static async Task SyncBeatmapDatabaseAsync()
        {
            // Find all .qua files in the songs folder.
            var files = Directory.GetFiles(Configuration.SongDirectory, "*.qua", SearchOption.AllDirectories);

            // This will hold all of the md5 checksums of all of the .qua files in the directory
            // we will check if they are all in the database and not changed.
            var md5Checksums = new List<string>();
            files.ToList().ForEach(x => md5Checksums.Add(Beatmap.GetMd5Checksum(x)));
            Console.WriteLine($"{module} Found {files.Length} .qua files in the /Songs/ Directory!");

            // Find all of the beatmaps currently in the database, and check if the amount matches the # of found files.
            var conn = new SQLiteAsyncConnection(DatabasePath);
            var query = conn.Table<Beatmap>();

            await query.ToListAsync().ContinueWith(async t =>
            {
                var foundMaps = t.Result;
                Console.WriteLine($"{module} Found {foundMaps.Count} beatmaps in the database!");

                if (foundMaps.Count != files.Length)
                {
                    Console.WriteLine(
                        $"{module} Incorrect number of .qua files vs maps detected. {files.Length} vs {foundMaps.Count}!");

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
                                $"{module} Error: .Qua could not successfully be parsed: {quaFile}!");
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

                // Check if the MD5 Hashes of the .qua files are all in the database. If they're not, we'll have to 
                // reprocess and update that individual beatmap.
                var changedBeatmaps = t.Result
                    .Except(t.Result.Where(map => md5Checksums.Any(md5 => md5 == map.Md5Checksum)).ToList()).ToList();

                // If there's any changed maps, want to reparse them and update them.
                if (changedBeatmaps.Count > 0)
                    changedBeatmaps.ForEach(async cb =>
                    {
                        await conn.DeleteAsync(cb);

                        // Parse the map again, and get all the updated values.
                        var reprocessedMap = new Beatmap().ConvertQuaToBeatmap(new Qua(cb.Path), cb.Path);

                        // If the beatmap is valid, then we'll add it to the maps to be refreshed.
                        if (!reprocessedMap.IsValidBeatmap)
                            return;

                        Console.WriteLine(
                            $"{module} MD5 Of Beatmap: {cb.Artist} - {cb.Title} ({cb.DifficultyName}) has changed." +
                            $" updating the database with the new values.");
                        await conn.InsertAsync(reprocessedMap);
                    });
            });

            // Now that the file count is synced, let's remove the records that don't have attached beatmaps.
            await SyncMissingQuaFromDatabase(files);
            Console.WriteLine($"{module} Beatmap synchronization completed!");
        }

        /// <summary>
        ///     Adds a list of beatmaps to the database
        /// </summary>
        /// <returns></returns>
        private static async Task AddBeatmapsToDatabase(List<Beatmap> beatmaps)
        {
            Console.WriteLine($"{module} Adding {beatmaps.Count} beatmaps to the database...");
            var conn = new SQLiteAsyncConnection(DatabasePath);

            var watch = Stopwatch.StartNew();

            await conn.InsertAllAsync(beatmaps).ContinueWith(t =>
                Console.WriteLine($"{module} Successfully added {beatmaps.Count} beatmaps to the database."));

            watch.Stop();
            Console.WriteLine($"{module} Beatmap importing took {watch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        ///     Responsible for removing any beatmaps from the database that don't have valid qua files.
        /// </summary>
        /// <returns></returns>
        private static async Task SyncMissingQuaFromDatabase(string[] files)
        {
            var conn = new SQLiteAsyncConnection(DatabasePath);
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
                if (mapsToRemove.Count > 0)
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
            Console.WriteLine($"{module} Removing {beatmaps.Count} beatmaps from the database...");

            var conn = new SQLiteConnection(DatabasePath);

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