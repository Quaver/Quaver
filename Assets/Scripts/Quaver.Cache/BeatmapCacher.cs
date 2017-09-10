using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data; 
using Mono.Data.Sqlite;

namespace Quaver.Cache
{
	public class BeatmapCacher : MonoBehaviour {

		private string connectionString;
		
		// Use this for initialization
		void Start () {
			// Where quaver.db is stored.
			connectionString = "URI=file:" + Application.dataPath + "/quaver.db";
			//GetBeatmaps();
			//WriteNewBeatmap(Application.dataPath, Application.dataPath + "\\memes.qua", -1, -1, "Camellias", "Backbeat Maniacs", 5.28f, 'S', 0, DateTime.Now);
			DeleteBeatmap(@"C:\Beatmap.qua");
		}
		
		// Update is called once per frame
		void Update () {
			
		}

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
							Debug.Log(reader.GetString(2));
						}

						dbConnection.Close();
						reader.Close();
					}
				}
			}
		}
	}
}

