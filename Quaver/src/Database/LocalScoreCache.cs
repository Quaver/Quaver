﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Database.Beatmaps;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Scores;
using SQLite;

namespace Quaver.Database
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
                Logger.Log($"Local Scores Database has been created.", Color.Cyan);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red);
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
                Logger.Log(e.Message, Color.Red);
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
                Logger.Log(e.Message, Color.Red);
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
                Logger.Log($"Successfully deleted score from beatmap: {score.BeatmapMd5} from the database.", Color.Cyan);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red);
            }
        }
    }
}
