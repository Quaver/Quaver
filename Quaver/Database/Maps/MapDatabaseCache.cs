using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaver.API.Maps;
using Quaver.Config;
using SQLite;
using Wobble.Logging;

namespace Quaver.Database.Maps
{
    public static class MapDatabaseCache
    {
        /// <summary>
        ///     The path of the local database
        /// </summary>
        private static readonly string DatabasePath = ConfigManager.GameDirectory + "/quaver.db";

        /// <summary>
        ///     Loads all of the maps in the database and groups them into mapsets to use
        ///     for gameplay
        /// </summary>
        public static void Load(bool fullSync)
        {
            CreateTable();

            if (fullSync)
                PerformFullSync();

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
        ///     Syncs the cache and makes sure that it is up-to-date
        /// </summary>
        private static void PerformFullSync()
        {
            // Fetch all of the .qua files inside of the song directory
            var quaFiles = Directory.GetFiles(ConfigManager.SongDirectory.Value, "*.qua", SearchOption.AllDirectories).ToList();
            Logger.Important($"Found {quaFiles.Count} .qua files inside the song directory", LogType.Runtime);

            SyncMissingFiles(quaFiles);
        }

        /// <summary>
        ///     Checks the maps in the database vs. the amount of .qua files on disk.
        ///     If there's a mismatch, it will add any missing ones
        /// </summary>
        private static void SyncMissingFiles(List<string> files)
        {
            var maps = FetchAll();

            // Stores the list of missing maps to be cached
            var mapsToCache = new List<Map>();

            foreach (var file in files)
            {
                // Check to see if the map exists in the cache already
                if (maps.Any(x => BackslashToForward($"{ConfigManager.SongDirectory.Value}/{x.Directory}/{x.Path}") == BackslashToForward(file)))
                    continue;

                try
                {
                    mapsToCache.Add(Map.FromQua(Qua.Parse(file), file));
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            Logger.Important($"Found {mapsToCache.Count} maps that need to be cached", LogType.Runtime);
        }

        /// <summary>
        ///     Responsible for fetching all the maps from the database and returning them.
        /// </summary>
        /// <returns></returns>
        private static List<Map> FetchAll() => new SQLiteConnection(DatabasePath).Table<Map>().ToList();

        /// <summary>
        ///     Converts all backslash characters to forward slashes.
        ///     Used for paths
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static string BackslashToForward(string p) => p.Replace("\\", "/");

        /// <summary>
        ///     Inserts a list of given maps into the database
        /// </summary>
        /// <param name="maps"></param>
        private static void InsertMaps(IReadOnlyCollection<Map> maps)
        {
            if (maps.Count == 0)
                return;

            foreach (var map in maps)
            {
                try
                {
                    new SQLiteConnection(DatabasePath).InsertOrReplace(map);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        ///     Inserts an individual map to the database.
        /// </summary>
        /// <param name="map"></param>
        public static void InsertMap(Map map)
        {
            try
            {
                new SQLiteConnection(DatabasePath).Insert(map);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Fetches all maps, groups them into mapsets, sets them to allow them to be played.
        /// </summary>
        public static void OrderAndSetMapsets()
        {
            var mapsets = MapsetHelper.ConvertMapsToMapsets(FetchAll());
            MapManager.Mapsets = MapsetHelper.OrderMapsByDifficulty(MapsetHelper.OrderMapsetsByArtist(mapsets));
        }
    }
}