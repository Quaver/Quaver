using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Config;
using System.IO;
using Quaver.Beatmaps;
using SQLite;

namespace Quaver.Database
{
    internal static class DatabaseHelper
    {
        /// <summary>
        /// Holds the path of the SQLite database
        /// </summary>
        internal static readonly string databasePath = Configuration.GameDirectory + "/quaver.db";

        /// <summary>
        /// Initialize the beatmap database & create a table named Beatmaps with the Beatmap class properties.
        /// </summary>
        internal static async Task InitializeBeatmapDatabaseAsync()
        {
            Console.WriteLine("[DATABASE HELPER] Initializing/Reading Beatmap Database...");

            var conn = new SQLiteAsyncConnection(databasePath);
            await conn.CreateTableAsync<Beatmap>();

            Console.WriteLine("[DATABASE HELPER] Successfully initialized database & created table: \"Beatmaps\"");
        }
    }
}
