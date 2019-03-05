using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Shared;
using osu_database_reader.BinaryFiles;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens;
using SQLite;
using UniversalThreadManagement;
using Wobble;
using Wobble.Logging;
using GameMode = osu.Shared.GameMode;

namespace Quaver.Shared.Database.Maps
{
    public static class OtherGameMapDatabaseCache
    {
        /// <summary>
        ///     The path of the local database
        /// </summary>
        public static readonly string DatabasePath = ConfigManager.GameDirectory + "/quaver.db";

        /// <summary>
        /// </summary>
        public static Dictionary<OtherGameCacheAction, List<OtherGameMap>> MapsToCache { get; private set; }

        /// <summary>
        ///     Dictates if we need to update the
        /// </summary>
        public static bool NeedsSync => ConfigManager.AutoLoadOsuBeatmaps.Value && (MapsToCache[OtherGameCacheAction.Delete].Count > 0 ||
                                               MapsToCache[OtherGameCacheAction.Add].Count > 0 || MapsToCache[OtherGameCacheAction.Update].Count > 0);

        /// <summary>
        ///     The thread that the sync will take place on
        /// </summary>
        public static Thread Thread { get; private set; }

        /// <summary>
        ///     Returns if the cache is currently being synced.
        /// </summary>
        public static bool IsSyncing => Thread != null && Thread.IsAlive;

        /// <summary>
        ///     If we're currently able to sync maps.
        /// </summary>
        public static bool EligibleToSync => ConfigManager.AutoLoadOsuBeatmaps.Value && NeedsSync && OnSyncableScreen();

        /// <summary>
        ///     The amount of maps left to sync
        /// </summary>
        public static int SyncMapCount => MapsToCache[OtherGameCacheAction.Delete].Count + MapsToCache[OtherGameCacheAction.Add].Count
                                                + MapsToCache[OtherGameCacheAction.Update].Count;
        /// <summary>
        ///   Loads maps from other games if necessary
        /// </summary>
        public static List<OtherGameMap> Load()
        {
            CreateTable();
            return SyncMaps();
        }

        /// <summary>
        ///     Runs the worker thread for the sync process
        /// </summary>
        public static void RunThread()
        {
            if (!EligibleToSync)
                return;

            if (MapsToCache == null && ConfigManager.AutoLoadOsuBeatmaps.Value)
                MapDatabaseCache.OrderAndSetMapsets();

            if (IsSyncing || !NeedsSync)
                return;

            Thread = new Thread(() =>
            {
                if (MapsToCache != null && NeedsSync)
                {
                    NotificationManager.Show(NotificationLevel.Info,
                        $"Calculating difficulty ratings of other games' maps in the background. {SyncMapCount} maps left!");

                    Logger.Important($"Starting other game sync thread.", LogType.Runtime);
                }
                else
                    return;

                using (var conn = new SQLiteConnection(DatabasePath))
                {
                    AddMaps(conn);
                    UpdateMaps(conn);
                    DeleteMaps(conn);

                    if (SyncMapCount == 0)
                        NotificationManager.Show(NotificationLevel.Success, "Successfully completed difficulty rating calculations for other games!");
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };

            Thread.Start();
        }

        /// <summary>
        ///
        /// </summary>
        private static void CreateTable()
        {
            try
            {
                var conn = new SQLiteConnection(DatabasePath);
                conn.CreateTable<OtherGameMap>();

                Logger.Important($"OtherGameMaps table has been created.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static List<OtherGameMap> FetchAll() => new SQLiteConnection(DatabasePath).Table<OtherGameMap>().ToList();

        /// <summary>
        /// </summary>
        private static List<OtherGameMap> SyncMaps()
        {
            Logger.Important($"Starting sync of other game maps...", LogType.Runtime);

            MapsToCache = new Dictionary<OtherGameCacheAction, List<OtherGameMap>>()
            {
                {OtherGameCacheAction.Add, new List<OtherGameMap>()},
                {OtherGameCacheAction.Update, new List<OtherGameMap>()},
                {OtherGameCacheAction.Delete, new List<OtherGameMap>()}
            };

            var currentlyCached = FetchAll();
            var osuMaps = LoadOsuBeatmapDatabase();

            // Find maps that need to be deleted/updated from the cache
            for (var i = currentlyCached.Count - 1; i >= 0; i--)
            {
                var map = currentlyCached[i];

                if (osuMaps.All(x => x.Md5Checksum != map.Md5Checksum))
                {
                    MapsToCache[OtherGameCacheAction.Delete].Add(map);
                    currentlyCached.Remove(map);
                    continue;
                }

                if (map.DifficultyProcessorVersion != DifficultyProcessorKeys.Version)
                    MapsToCache[OtherGameCacheAction.Update].Add(map);
            }

            // Find maps that need to be added to the database.
            foreach (var map in osuMaps)
            {
                if (currentlyCached.Find(x => x.Md5Checksum == map.Md5Checksum) != null)
                    continue;

                MapsToCache[OtherGameCacheAction.Add].Add(map);
                currentlyCached.Add(map);
            }

            currentlyCached.ForEach(x => x.Game = MapGame.Osu);
            return currentlyCached;
        }

         /// <summary>
        ///     Reads the osu!.db file defined in config and loads all of those maps into the cache.
        /// </summary>
        private static List<OtherGameMap> LoadOsuBeatmapDatabase()
        {
            try
            {
                var db = OsuDb.Read(ConfigManager.OsuDbPath.Value);
                MapManager.OsuSongsFolder = Path.GetDirectoryName(ConfigManager.OsuDbPath.Value) + "/Songs/";

                // Find all osu! maps that are 4K and 7K and order them by their difficulty value.
                var osuBeatmaps = db.Beatmaps.Where(x => x.GameMode == GameMode.Mania && ( x.CircleSize == 4 || x.CircleSize == 7 )).ToList();
                osuBeatmaps = osuBeatmaps.OrderBy(x => x.DiffStarRatingMania.ContainsKey(Mods.None) ? x.DiffStarRatingMania[Mods.None] : 0).ToList();

                var osuToQuaverMaps = new List<OtherGameMap>();

                foreach (var map in osuBeatmaps)
                {
                    var newMap = new OtherGameMap()
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
                        RegularNoteCount = map.CountHitCircles,
                        LongNoteCount = map.CountSliders,
                        LocalOffset = map.OffsetLocal
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

                return new List<OtherGameMap>();
            }
        }

        /// <summary>
        ///     Returns if the user is on a screen where syncing can occur
        /// </summary>
        /// <returns></returns>
        private static bool OnSyncableScreen()
        {
            var game = (QuaverGame) GameBase.Game;

            switch (game.CurrentScreen.Type)
            {
                case QuaverScreenType.Editor:
                case QuaverScreenType.Gameplay:
                case QuaverScreenType.Loading:
                case QuaverScreenType.Alpha:
                case QuaverScreenType.Importing:
                case QuaverScreenType.Results:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        ///     Adds all maps in the cache to the database
        /// </summary>
        private static void AddMaps(SQLiteConnection conn)
        {
            for (var i = MapsToCache[OtherGameCacheAction.Add].Count - 1; i >= 0; i--)
            {
                if (!EligibleToSync)
                    break;

                var map = MapsToCache[OtherGameCacheAction.Add][i];

                try
                {
                    map.DifficultyProcessorVersion = DifficultyProcessorKeys.Version;
                    map.CalculateDifficulties();
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }
                finally
                {
                    // We want to insert regardless, but still pop it under another try/catch
                    // to avoid potential failures
                    try
                    {
                        conn.Insert(map);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception)
                    {
                    }

                    MapsToCache[OtherGameCacheAction.Add].Remove(map);
                }
            }
        }

        /// <summary>
        ///     Updates all existing maps
        /// </summary>
        private static void UpdateMaps(SQLiteConnection conn)
        {
            for (var i = MapsToCache[OtherGameCacheAction.Update].Count - 1; i >= 0; i--)
            {
                if (!EligibleToSync)
                    break;

                var map = MapsToCache[OtherGameCacheAction.Update][i];

                try
                {
                    map.DifficultyProcessorVersion = DifficultyProcessorKeys.Version;
                    map.CalculateDifficulties();
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }
                finally
                {
                    try
                    {
                        conn.Update(map);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception e)
                    {
                    }

                    MapsToCache[OtherGameCacheAction.Update].Remove(map);
                }
            }
        }

        /// <summary>
        ///     Removes missing maps
        /// </summary>
        private static void DeleteMaps(SQLiteConnection conn)
        {
            for (var i = MapsToCache[OtherGameCacheAction.Delete].Count - 1; i >= 0; i--)
            {
                if (!EligibleToSync)
                    break;

                var map = MapsToCache[OtherGameCacheAction.Delete][i];

                try
                {
                    conn.Delete(map);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }
                finally
                {
                    MapsToCache[OtherGameCacheAction.Delete].Remove(map);
                }
            }
        }
    }
}
