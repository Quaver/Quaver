using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using IniFileParser;
using Microsoft.Win32;
using osu.Shared;
using osu_database_reader.BinaryFiles;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps.Etterna;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
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
        /// </summary>
        public static Dictionary<OtherGameCacheAction, List<Map>> MapsToCache { get; private set; }

        /// <summary>
        ///     Dictates if we need to update the
        /// </summary>
        public static bool NeedsSync
        {
            get
            {
                if (MapsToCache == null)
                    return false;

                try
                {
                    return MapsToCache[OtherGameCacheAction.Delete]?.Count > 0 ||
                           MapsToCache[OtherGameCacheAction.Add]?.Count > 0 ||
                           MapsToCache[OtherGameCacheAction.Update]?.Count > 0;
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    return false;
                }
            }
        }

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
        public static bool EligibleToSync => NeedsSync && OnSyncableScreen();

        /// <summary>
        ///     The amount of maps left to sync
        /// </summary>
        public static int SyncMapCount => MapsToCache[OtherGameCacheAction.Delete].Count + MapsToCache[OtherGameCacheAction.Add].Count
                                                + MapsToCache[OtherGameCacheAction.Update].Count;

        public static void Initialize()
        {
            MapsToCache = new Dictionary<OtherGameCacheAction, List<Map>>()
            {
                {OtherGameCacheAction.Add, new List<Map>()},
                {OtherGameCacheAction.Update, new List<Map>()},
                {OtherGameCacheAction.Delete, new List<Map>()}
            };
        }

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
                        $"Syncing outdated maps in the background. Performance may be lower. {SyncMapCount} maps left!");

                    Logger.Important($"Starting other game sync thread.", LogType.Runtime);
                }
                else
                    return;

                AddMaps(DatabaseManager.Connection);
                UpdateMaps(DatabaseManager.Connection);
                DeleteMaps(DatabaseManager.Connection);

                if (SyncMapCount == 0)
                    NotificationManager.Show(NotificationLevel.Success, "Successfully completed syncing outdated maps!");
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
                DatabaseManager.Connection.CreateTable<OtherGameMap>();

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
        public static List<OtherGameMap> FetchAll() => DatabaseManager.Connection.Table<OtherGameMap>().ToList();

        /// <summary>
        /// </summary>
        private static List<OtherGameMap> SyncMaps()
        {
            Logger.Important($"Starting sync of other game maps...", LogType.Runtime);

            var currentlyCached = FetchAll();

            var osuMaps = new List<OtherGameMap>();
            var etternaCharts = new List<OtherGameMap>();

            if (!string.IsNullOrEmpty(ConfigManager.OsuDbPath.Value))
                osuMaps = LoadOsuBeatmapDatabase();

            if (!string.IsNullOrEmpty(ConfigManager.EtternaDbPath.Value))
                etternaCharts = EtternaDatabaseCache.Load();

            // Remove all osu! cached maps if the db isn't loaded
            if (string.IsNullOrEmpty(ConfigManager.OsuDbPath.Value) || osuMaps.Count == 0)
                currentlyCached.RemoveAll(x => x.OriginalGame == OtherGameMapDatabaseGame.Osu);

            // Remove all ett charts if the db isn't loaded
            if (string.IsNullOrEmpty(ConfigManager.EtternaDbPath.Value) || etternaCharts.Count == 0)
                currentlyCached.RemoveAll(x => x.OriginalGame == OtherGameMapDatabaseGame.Etterna);

            // Make sure there're no duplicate Checksums
            osuMaps = ListHelper.DistinctBy(osuMaps, x => x.Md5Checksum).ToList();
            etternaCharts = ListHelper.DistinctBy(etternaCharts, x => x.Md5Checksum).ToList();

            // Creating hash objects
            var osuMapsHash = osuMaps.ToDictionary(x => x.Md5Checksum);
            var etternaChartsHash = etternaCharts.ToDictionary(x => x.Md5Checksum);
            var currentlyCachedHash = currentlyCached.Select(x => x.Md5Checksum).ToHashSet();

            // Find maps that need to be deleted/updated from the cache
            for (var i = currentlyCached.Count - 1; i >= 0; i--)
            {
                var map = currentlyCached[i];

                if (!osuMapsHash.ContainsKey(map.Md5Checksum) && !etternaChartsHash.ContainsKey(map.Md5Checksum))
                {
                    if (map.OriginalGame == OtherGameMapDatabaseGame.Osu && osuMaps.Count != 0
                        || map.OriginalGame == OtherGameMapDatabaseGame.Etterna && etternaCharts.Count != 0)
                    {
                        MapsToCache[OtherGameCacheAction.Delete].Add(map);
                        currentlyCached.Remove(map);
                    }

                    continue;
                }

                // Updates for osu
                if (map.OriginalGame == OtherGameMapDatabaseGame.Osu)
                {
                    // Update directory and path if changed
                    var refMap = osuMapsHash[map.Md5Checksum];
                    if (refMap != null && (refMap.Directory != map.Directory || refMap.Path != map.Path))
                    {
                        MapsToCache[OtherGameCacheAction.Update].Add(map);
                        currentlyCached[i] = refMap;
                    }
                }
            }

            // Find maps that need to be added to the database.
            foreach (var map in osuMaps)
            {
                if (!currentlyCachedHash.Contains(map.Md5Checksum))
                {
                    MapsToCache[OtherGameCacheAction.Add].Add(map);
                    currentlyCached.Add(map);
                }
            }

            // Find maps that need to be added to the database.
            foreach (var map in etternaCharts)
            {
                if (!currentlyCachedHash.Contains(map.Md5Checksum))
                {
                    MapsToCache[OtherGameCacheAction.Add].Add(map);
                    currentlyCached.Add(map);
                }
            }

            currentlyCached.ForEach(x =>
            {
                var diffOutdated = x.DifficultyProcessorVersion != DifficultyProcessorKeys.Version;
                var versionOutdated = false;

                switch (x.OriginalGame)
                {
                    case OtherGameMapDatabaseGame.Osu:
                        x.Game = MapGame.Osu;
                        versionOutdated = OtherGameMap.OsuSyncVersion != x.SyncVersion;
                        break;
                    case OtherGameMapDatabaseGame.Etterna:
                        x.Game = MapGame.Etterna;
                        versionOutdated = OtherGameMap.EtternaSyncVersion != x.SyncVersion;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if ((diffOutdated || versionOutdated) && !MapsToCache[OtherGameCacheAction.Add].Contains(x))
                    MapsToCache[OtherGameCacheAction.Update].Add(x);
            });

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

                var osuFolder = Path.GetDirectoryName(ConfigManager.OsuDbPath.Value);

                // Default songs path
                MapManager.OsuSongsFolder = osuFolder + "/Songs/";

                // Read the config file and set the appropriate path
                var configFile = $"{osuFolder}/osu!.{Environment.UserName}.cfg";

                if (File.Exists(configFile))
                {
                    try
                    {
                        foreach (var line in File.ReadAllLines(configFile))
                        {
                            if (!line.StartsWith("BeatmapDirectory"))
                                continue;

                            var dir = line.Split("=")[1].Trim();

                            MapManager.OsuSongsFolder = Directory.Exists(dir) ? dir + "/" : $"{osuFolder}/{dir}/";
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                    }
                }

                // Find all osu! maps that are 4K and 7K and order them by their difficulty value.
                var osuBeatmaps = db.Beatmaps.Where(x => x.GameMode == GameMode.Mania && (x.CircleSize == 4 || x.CircleSize == 7  || x.CircleSize == 8)).ToList();
                osuBeatmaps = osuBeatmaps.OrderBy(x => x.DiffStarRatingMania.ContainsKey(Mods.None) ? x.DiffStarRatingMania[Mods.None] : 0).ToList();

                var osuToQuaverMaps = new List<OtherGameMap>();

                foreach (var map in osuBeatmaps)
                {
                    var newMap = new OtherGameMap()
                    {
                        Md5Checksum = map.BeatmapChecksum,
                        Directory = map.FolderName.Trim(),
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
                        OriginalGame = OtherGameMapDatabaseGame.Osu,
                        BackgroundPath = "",
                        RegularNoteCount = map.CountHitCircles,
                        LongNoteCount = map.CountSliders,
                        LocalOffset = map.OffsetLocal,
                        HasScratchKey = map.CircleSize == 8
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
        public static bool OnSyncableScreen()
        {
            var game = (QuaverGame) GameBase.Game;

            switch (game.CurrentScreen?.Type)
            {
                case QuaverScreenType.Editor:
                case QuaverScreenType.Gameplay:
                case QuaverScreenType.Loading:
                case QuaverScreenType.Alpha:
                case QuaverScreenType.Importing:
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

                    // Etterna doesn't store the object count/common bpm, so add it here.
                    if (map is OtherGameMap ogm && ogm.OriginalGame == OtherGameMapDatabaseGame.Etterna)
                    {
                        var qua = map.LoadQua();

                        map.LongNoteCount = qua.HitObjects.FindAll(x => x.IsLongNote).Count;
                        map.RegularNoteCount = qua.HitObjects.FindAll(x => !x.IsLongNote).Count;
                        map.Bpm = qua.GetCommonBpm();
                        map.SongLength = qua.Length;
                    }
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
                        switch (map.Game)
                        {
                            case MapGame.Quaver:
                                conn.Insert(map);
                                break;
                            case MapGame.Osu:
                                var osuMap = (OtherGameMap) map;
                                osuMap.SyncVersion = OtherGameMap.OsuSyncVersion;
                                conn.Insert(osuMap);
                                break;
                            case MapGame.Etterna:
                                var etternaChart = (OtherGameMap) map;
                                etternaChart.SyncVersion = OtherGameMap.EtternaSyncVersion;
                                conn.Insert(etternaChart);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
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

                    // Etterna doesn't store the object count/common bpm, so add it here.
                    if (map is OtherGameMap ogm && ogm.OriginalGame == OtherGameMapDatabaseGame.Etterna)
                    {
                        var qua = map.LoadQua();

                        map.LongNoteCount = qua.HitObjects.FindAll(x => x.IsLongNote).Count;
                        map.RegularNoteCount = qua.HitObjects.FindAll(x => !x.IsLongNote).Count;
                        map.Bpm = qua.GetCommonBpm();
                        map.SongLength = qua.Length;
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }
                finally
                {
                    try
                    {
                        switch (map.Game)
                        {
                            case MapGame.Quaver:
                                conn.Update(map);
                                break;
                            case MapGame.Osu:
                                var osuMap = (OtherGameMap) map;
                                osuMap.SyncVersion = OtherGameMap.OsuSyncVersion;
                                conn.Update(osuMap);
                                break;
                            case MapGame.Etterna:
                                var etternaChart = (OtherGameMap) map;
                                etternaChart.SyncVersion = OtherGameMap.EtternaSyncVersion;
                                conn.Update(etternaChart);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
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
                    switch (map.Game)
                    {
                        case MapGame.Quaver:
                            conn.Delete(map);
                            break;
                        case MapGame.Osu:
                        case MapGame.Etterna:
                            conn.Delete((OtherGameMap) map);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
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

        /// <summary>
        ///     Tries to find a user's osu! installation if their db path isn't already set
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void FindOsuStableInstallation()
        {
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey("osu"))
                {
                    var value = key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty).ToString();
                    var installPath = value?.Split('"')[1]?.Replace("osu!.exe", "");

                    if (string.IsNullOrEmpty(installPath))
                        return;

                    ConfigManager.OsuDbPath.Value = $"{installPath}/osu!.db";

                    Logger.Important($"Successfully detected osu! installation at path: {ConfigManager.OsuDbPath.Value}",
                        LogType.Runtime);
                }
            }
            catch (Exception e)
            {
                Logger.Warning($"Failed to find osu! installation in the registry", LogType.Runtime);
            }
        }

        /// <summary>
        ///     Tries to find a user's etterna installation if their db path isn't already set
        /// </summary>
        public static void FindEtternaInstallation()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("Wow6432Node"))
                {
                    var etterna = key?.OpenSubKey("Etterna Team")?.OpenSubKey("Etterna")?.GetValue(string.Empty).ToString();

                    if (string.IsNullOrEmpty(etterna))
                        return;

                    ConfigManager.EtternaDbPath.Value = $"{etterna}/Cache/cache.db";

                    Logger.Important($"Successfully detected Etterna installation at path: {ConfigManager.EtternaDbPath.Value}",
                        LogType.Runtime);
                }
            }
            catch (Exception e)
            {
                Logger.Warning($"Failed to find Ett installation in the registry", LogType.Runtime);
            }
        }
    }
}
