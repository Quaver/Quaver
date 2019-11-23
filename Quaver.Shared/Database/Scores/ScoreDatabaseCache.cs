/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using SQLite;
using Wobble.Logging;

namespace Quaver.Shared.Database.Scores
{
    public static class ScoreDatabaseCache
    {
         /// <summary>
         ///     Event invoked when a score has been deleted
         /// </summary>
         public static event EventHandler<ScoreDeletedEventArgs> ScoreDeleted;

         /// <summary>
         ///     Event invoked when a map's local scores have been deleted
         /// </summary>
        public static event EventHandler<LocalScoresDeletedEventArgs> LocalMapScoresDeleted;


        /// <summary>
        ///     Asynchronously creates the scores database
        /// </summary>
        /// <returns></returns>
        internal static void CreateTable()
        {
            try
            {
                DatabaseManager.Connection.CreateTable<Score>();
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
                var sql = $"SELECT * FROM 'Score' WHERE MapMd5=? ORDER BY TotalScore DESC LIMIT 50";
                var scores = DatabaseManager.Connection.Query<Score>(sql, md5);

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
                DatabaseManager.Connection.Insert(score);
                return score.Id;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }

            return -1;
        }

        /// <summary>
        ///     Responsible for removing a score from the database
        /// </summary>
        /// <param name="score"></param>
        /// <param name="raiseEvent"></param>
        /// <returns></returns>
        internal static void DeleteScoreFromDatabase(Score score, bool raiseEvent = true)
        {
            try
            {
                DatabaseManager.Connection.Delete(score);

                if (raiseEvent)
                    ScoreDeleted?.Invoke(typeof(ScoreDatabaseCache), new ScoreDeletedEventArgs(score));

                Logger.Important($"Successfully deleted score from map: {score.MapMd5} from the database.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Deletes all local scores for a map
        /// </summary>
        /// <param name="map"></param>
        internal static void DeleteAllLocalScores(Map map)
        {
            try
            {
                var scores = FetchMapScores(map.Md5Checksum);
                scores.ForEach(x => DeleteScoreFromDatabase(x, false));
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            LocalMapScoresDeleted?.Invoke(typeof(ScoreDatabaseCache), new LocalScoresDeletedEventArgs(map));
        }
    }
}
