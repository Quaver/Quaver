using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Config;
using SQLite;
using Wobble.Logging;

namespace Quaver.Database.Scores
{
    public static class LocalScoreCache
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
                conn.CreateTable<LocalScore>();

                Logger.Important("LocalScores SQLite table has been created", LogType.Runtime);
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
        internal static List<LocalScore> FetchMapScores(string md5)
        {
            try
            {
                var conn = new SQLiteConnection(DatabasePath);
                var sql = $"SELECT * FROM 'LocalScore' WHERE MapMd5=? ORDER BY Score DESC LIMIT 50";

                var scores = conn.Query<LocalScore>(sql, md5);
                conn.Close();

                return scores.OrderBy(x => x.Grade == Grade.F).ThenByDescending(x => x.Score).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return new List<LocalScore>();
            }
        }

        /// <summary>
        ///     Responsible for inserting a score into the database
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        internal static int InsertScoreIntoDatabase(LocalScore score)
        {
            try
            {
                if (score != null)
                    new SQLiteConnection(DatabasePath).Insert(score);

                return new SQLiteConnection(DatabasePath).Table<LocalScore>().Count();
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
        internal static void DeleteScoreFromDatabase(LocalScore score)
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