/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using SQLite;
using Wobble.Logging;

namespace Quaver.Shared.Database.Scores
{
    public static class ScoreDatabaseCache
    {
         /// <summary>
        ///     The path of the local scores database
        /// </summary>
        private static readonly string DatabasePath = $"{ConfigManager.GameDirectory}/quaver.db";

        /// <summary>
        ///     Asynchronously creates the scores database
        /// </summary>
        /// <returns></returns>
        internal static void CreateTable()
        {
            try
            {
                var conn = new SQLiteConnection(DatabasePath);
                conn.CreateTable<Score>();

                Logger.Important("Scores table has been created", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Grabs all the local scores from a particular map by MD5 hash
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        internal static List<Score> FetchMapScores(string md5)
        {
            try
            {
                var conn = new SQLiteConnection(DatabasePath);
                var sql = $"SELECT * FROM 'Score' WHERE MapMd5=? ORDER BY TotalScore DESC LIMIT 50";

                var scores = conn.Query<Score>(sql, md5);
                conn.Close();

                // Remove all scores that have F grade if "Display Failed Scores" setting is set to yes.
                if (!ConfigManager.DisplayFailedLocalScores.Value)
                    scores.RemoveAll(x => x.Grade == Grade.F);
                
                return scores.OrderBy(x => x.Grade == Grade.F).ThenByDescending(x => x.PerformanceRating).ThenByDescending(x => x.Accuracy).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return new List<Score>();
            }
        }

        /// <summary>
        ///     Responsible for inserting a score into the database
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        internal static int InsertScoreIntoDatabase(Score score)
        {
            try
            {
                if (score != null)
                    new SQLiteConnection(DatabasePath).Insert(score);

                return new SQLiteConnection(DatabasePath).Table<Score>().Count();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Responsible for removing a score from the database
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        internal static void DeleteScoreFromDatabase(Score score)
        {
            try
            {
                new SQLiteConnection(DatabasePath).Delete(score);
                Logger.Important($"Successfully deleted score from map: {score.MapMd5} from the database.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}
