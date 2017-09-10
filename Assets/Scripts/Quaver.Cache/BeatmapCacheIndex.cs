using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data; 
using Mono.Data.Sqlite;

namespace Quaver.Cache
{
	public class BeatmapCacheIndex
	{
		private static string connectionString = "URI=file:" + Application.dataPath + "/quaver.db";

		// Upon game launch, we'll use this to create the database and table if it does not already exist.
		// This is ran before anything else.
		public static void CreateDatabase()
		{
			// Create db connection
			using(IDbConnection dbConnection = new SqliteConnection(connectionString))
			{
				// Open db connection
				dbConnection.Open();

				// Use connection to create an SQL Query we can execute
				using(IDbCommand dbCmd = dbConnection.CreateCommand())
				{
					string query = "CREATE TABLE if not exists \"main\".\"beatmaps\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , \"directory\" TEXT NOT NULL , \"path\" TEXT NOT NULL  UNIQUE , \"beatmapsetid\" INTEGER NOT NULL , \"beatmapid\" INTEGER NOT NULL , \"artist\" TEXT NOT NULL , \"title\" TEXT NOT NULL , \"difficulty\" INTEGER NOT NULL , \"rank\" TEXT NOT NULL , \"status\" INTEGER NOT NULL  DEFAULT 0, \"lastplayed\" DATETIME NOT NULL  DEFAULT CURRENT_DATE)";

					dbCmd.CommandText = query;
					dbCmd.ExecuteScalar(); // Execute scalar when inserting
					dbConnection.Close();
					Debug.Log("[CACHE] Beatmap Database Loaded/Generated!");
				}
			}	
		}

		// Upon game launch, we'll want to load all of the beatmaps in the database to a List<CachedBeatmap>
		// These will be the maps that we will be available for song select.
		public static List<CachedBeatmap> LoadBeatmaps()
		{
			List<CachedBeatmap> loadedBeatmaps = new List<CachedBeatmap>();

			// First we'll want to find all beatmaps in the database.
			// Create db connection
			using(IDbConnection dbConnection = new SqliteConnection(connectionString))
			{
				// Open db connection
				dbConnection.Open();

				// Use connection to create an SQL Query we can execute
				using(IDbCommand dbCmd = dbConnection.CreateCommand())
				{
					string query = "SELECT * FROM beatmaps";

					dbCmd.CommandText = query;

					// To be able to read the database data
					using (IDataReader reader = dbCmd.ExecuteReader())
					{
						// Reader stores scores by their index. So, in this particular db, reader.GetString(0) 
						// would return the id of the first row in the table.
						while (reader.Read())
						{
							// Create new Cached Beatmap Object
							CachedBeatmap loadedBeatmap = new CachedBeatmap(reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetInt32(4),
																reader.GetString(5), reader.GetString(6), reader.GetFloat(7), reader.GetString(8),
																reader.GetInt32(9), reader.GetDateTime(10));

							// Log that shit for now, because i already know some fucking thing is going to go wrong.
							Debug.Log(String.Format("[CACHE] Beatmap Loaded: Dir: {0}, Path: {1}, BSID: {2}, BID: {3}, Artist: {4}, Title: {5}, Diff: {6}, Rank: {7}, Status: {8}, LP: {9}", 
													loadedBeatmap.DirectoryMD5, loadedBeatmap.PathMD5, loadedBeatmap.BeatmapSetID, loadedBeatmap.BeatmapID,
													loadedBeatmap.Artist, loadedBeatmap.Title, loadedBeatmap.Difficulty, loadedBeatmap.Rank, loadedBeatmap.Status,
													loadedBeatmap.LastPlayed));

							// Append the loaded beatmap to our List
							loadedBeatmaps.Add(loadedBeatmap);
						}

						// Close DB & Reader
						dbConnection.Close();
						reader.Close();
					}
				}

			}			

			return loadedBeatmaps;
		}

	}

	/*(public class BeatmapCacher : MonoBehaviour {

		private string connectionString;
		

		void DeleteBeatmap(string filePath)
		{
			// Create db connection
			using(IDbConnection dbConnection = new SqliteConnection(connectionString))
			{
				// Open db connection
				dbConnection.Open();

				// Use connection to create an SQL Query we can execute
				using(IDbCommand dbCmd = dbConnection.CreateCommand())
				{
					string query = "DELETE FROM beatmaps WHERE path='" + filePath + "'";

					dbCmd.CommandText = query;

					dbCmd.ExecuteScalar(); // Execute scalar when inserting

					dbConnection.Close();
				}
			}				
		}

		void WriteNewBeatmap(string directory, string path, int beatmapSetId, int beatmapId, string artist, string title, float difficulty, char rank, int status, DateTime date)
		{
			// Create db connection
			using(IDbConnection dbConnection = new SqliteConnection(connectionString))
			{
				// Open db connection
				dbConnection.Open();

				// Use connection to create an SQL Query we can execute
				using(IDbCommand dbCmd = dbConnection.CreateCommand())
				{
					string query = String.Format("INSERT INTO beatmaps(directory,path,beatmapsetid,beatmapid,artist,title,difficulty,rank,status) " + 
									"VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')", 
									directory, path, beatmapSetId, beatmapId, artist, title, difficulty, rank, status);

					dbCmd.CommandText = query;

					dbCmd.ExecuteScalar(); // Execute scalar when inserting

					dbConnection.Close();
				}
			}			
		}

		void GetBeatmaps()
		{

		}
	}*/
}

