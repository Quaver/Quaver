/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Scheduling;
using SQLite;
using Wobble.Logging;

namespace Quaver.Shared.Database.Settings
{
    public static class QuaverSettingsDatabaseCache
    {
        /// <summary>
        ///     The path of the local database
        /// </summary>
        private static readonly string DatabasePath = ConfigManager.GameDirectory + "/quaver.db";

        /// <summary>
        ///     Current maps with outdated difficulties
        /// </summary>
        public static List<Map> OutdatedMaps { get; private set; }

        /// <summary>
        ///     Current scores with outdated ratings.
        /// </summary>
        public static List<Score> OutdatedScoreRatings { get; private set; }

        /// <summary>
        ///    Initializes the database, and either inserts/updates
        /// </summary>
        public static void Initialize()
        {
            CreateTable();

            using (var conn = new SQLiteConnection(DatabasePath))
            {
                var settings = conn.Find<QuaverSettings>(x => x.Id == 1);

                // Settings need to be updated
                if (settings == null)
                {
                    Logger.Important($"QuaverSettings could not be found previously in the database.", LogType.Runtime);

                    settings = new QuaverSettings
                    {
                        VersionDifficultyProcessorKeys = DifficultyProcessorKeys.Version,
                        VersionScoreProcessorKeys = ScoreProcessorKeys.Version,
                        VersionRatingProcessorKeys = RatingProcessorKeys.Version
                    };

                    conn.Insert(settings);
                }

                OutdatedMaps = MapDatabaseCache.FetchAll().FindAll(x => x.DifficultyProcessorVersion != DifficultyProcessorKeys.Version);
                Logger.Important($"Found {OutdatedMaps.Count} maps that have outdated difficulty ratings. Scheduling recalculation upon entering" +
                                 $"select.", LogType.Runtime);
            }
        }

        /// <summary>
        ///     Creates the `maps` database table.
        /// </summary>
        private static void CreateTable()
        {
            try
            {
                var conn = new SQLiteConnection(DatabasePath);
                conn.CreateTable<QuaverSettings>();
                Logger.Important($"QuaverSettings table has been created", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Does a recalculation for the difficulties on outdated maps.
        /// </summary>
        public static void RecalculateDifficultiesForOutdatedMaps()
        {
            foreach (var map in OutdatedMaps)
            {
                map.DifficultyProcessorVersion = DifficultyProcessorKeys.Version;

                try
                {
                    map.CalculateDifficulties();
                    MapDatabaseCache.UpdateMap(map);
                }
                catch (Exception e)
                {
                    new SQLiteConnection(DatabasePath).Delete(map);
                }
            }

            OutdatedMaps.Clear();
        }
    }
}
