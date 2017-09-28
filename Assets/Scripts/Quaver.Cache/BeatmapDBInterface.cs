using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using System.IO;
using System.Linq;
using Mono.Data.Sqlite;
using Quaver.Config;
using Quaver.Qua;
using Quaver.Utils;
using Quaver.Main;
using Quaver.SongSelect;
namespace Quaver.Cache
{
	public class BeatmapDBInterface
	{
        public static string s_connectionString = "URI=file:" + Application.dataPath + "/quaver.db";

        // Upon game launch, we'll use this to create the database and table if it does not already exist.
        // This is ran before anything else.
        public static void CreateDatabase()
        {
            // Create db connection
            using (IDbConnection dbConnection = new SqliteConnection(s_connectionString))
            {
                // Open db connection
                dbConnection.Open();

                // Use connection to create an SQL Query we can execute
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string query = "CREATE TABLE if not exists \"beatmaps\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , \"directory\" TEXT NOT NULL , \"path\" TEXT NOT NULL  UNIQUE , \"beatmapsetid\" INTEGER NOT NULL , \"beatmapid\" INTEGER NOT NULL , \"artist\" TEXT NOT NULL , \"title\" TEXT NOT NULL , \"difficulty\" TEXT NOT NULL , \"rank\" TEXT NOT NULL , \"status\" INTEGER NOT NULL  DEFAULT 0, \"lastplayed\" DATETIME NOT NULL  DEFAULT CURRENT_DATE, \"stars\" INTEGER NOT NULL, \"creator\" TEXT, \"backgroundpath\" TEXT, \"audiopath\" TEXT, \"audiopreviewtime\" INTEGER NOT NULL  DEFAULT 0, \"description\" TEXT, \"source\" TEXT, \"tags\" TEXT, \"bpm\" INTEGER NOT NULL  DEFAULT 0, \"songlength\" INTEGER NOT NULL  DEFAULT 0)";

                    dbCmd.CommandText = query;
                    dbCmd.ExecuteScalar(); // Execute scalar when inserting
                    dbConnection.Close();
                    Debug.Log("[CACHE] Beatmap Database Loaded/Generated!");
                }
            }
        }

        // This will be responsible for taking a CachedBeatmap and adding it to the database.
        public static void AddToDatabase(CachedBeatmap cachedMap)
        {
            using (IDbConnection dbConnection = new SqliteConnection(s_connectionString))
            {
                // Open db connection
                dbConnection.Open();

                // Use connection to create an SQL Query we can execute
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string query = String.Format("INSERT INTO beatmaps(directory,path,beatmapsetid,beatmapid,artist,title,difficulty,rank,status,lastplayed,stars,creator,backgroundpath,audiopath,audiopreviewtime,description,source,tags,bpm,songlength) " +
                                    "VALUES(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\", \"{10}\", \"{11}\", \"{12}\", \"{13}\", \"{14}\", \"{15}\", \"{16}\", \"{17}\", \"{18}\", \"{19}\")",
                                    cachedMap.Directory, cachedMap.Path, cachedMap.BeatmapSetID, cachedMap.BeatmapID,
                                    cachedMap.Artist.Replace("\"", String.Empty), cachedMap.Title.Replace("\"", String.Empty), cachedMap.Difficulty.Replace("\"", String.Empty), cachedMap.Rank.Replace("\"", String.Empty), cachedMap.Status,
                                    cachedMap.LastPlayed, cachedMap.Stars, cachedMap.Creator.Replace("\"", String.Empty), cachedMap.BackgroundPath, cachedMap.AudioPath, cachedMap.AudioPreviewTime,
                                    cachedMap.Description.Replace("\"", String.Empty), cachedMap.Source.Replace("\"", String.Empty), cachedMap.Tags.Replace("\"", String.Empty), cachedMap.BPM, cachedMap.SongLength);

                    dbCmd.CommandText = query;

                    dbCmd.ExecuteScalar(); // Execute scalar when inserting
                    dbConnection.Close();

                    Debug.Log("[CACHE] Added new beatmap to the cache.");
                }
            }
        }

        // This will be responsible for deleting a particular map from the database.
        public static void DeleteFromDatabase(CachedBeatmap mapToDelete)
        {
            // Create db connection
            using (IDbConnection dbConnection = new SqliteConnection(s_connectionString))
            {
                // Open db connection
                dbConnection.Open();

                // Use connection to create an SQL Query we can execute
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string query = "DELETE FROM beatmaps WHERE path=\"" + mapToDelete.Path + "\"";

                    dbCmd.CommandText = query;

                    dbCmd.ExecuteScalar(); // Execute scalar when inserting

                    dbConnection.Close();
                }
            }
        }
		
	}
}