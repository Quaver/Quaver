using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quaver.Config;
using Quaver.Logging;
using SQLite;

namespace Quaver.Database.Scores
{
    internal static class LocalScoreCache
    {
        /// <summary>
        ///     The path of the local scores database
        /// </summary>
        private static readonly string DatabasePath = $"{Configuration.GameDirectory}/scores.db";

        /// <summary>
        ///     Asynchronously creates the scores database
        /// </summary>
        /// <returns></returns>
        internal static async Task CreateScoresDatabase()
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DatabasePath);
                await conn.CreateTableAsync<LocalScore>();

                Logger.LogSuccess($"Local Scores Database has been created.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Grabs all the local scores from a particular beatmap by MD5 hash
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        internal static async Task<List<LocalScore>> SelectBeatmapScores(string md5)
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DatabasePath);         
                return await conn.Table<LocalScore>().Where(x => x.BeatmapMd5 == md5).ToListAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                return new List<LocalScore>();
            }
        }

        /// <summary>
        ///     Responsible for inserting a score into the database
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        internal static async Task InsertScoreIntoDatabase(LocalScore score)
        {
            try
            {
                if (score != null)
                    await new SQLiteAsyncConnection(DatabasePath).InsertAsync(score);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Responsible for removing a score from the database
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        internal static async Task DeleteScoreFromDatabase(LocalScore score)
        {
            try
            {
                await new SQLiteAsyncConnection(DatabasePath).DeleteAsync(score);
                Logger.LogSuccess($"Successfully deleted score from beatmap: {score.BeatmapMd5} from the database.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }
    }
}
