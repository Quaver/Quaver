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
			GetBeatmaps();		
		}
		
		// Update is called once per frame
		void Update () {
			
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

