
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
    public class BeatmapCacheIndex
    {
        private static string s_connectionString = "URI=file:" + Application.dataPath + "/quaver.db";

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
                    string query = "CREATE TABLE if not exists \"beatmaps\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , \"directory\" TEXT NOT NULL , \"path\" TEXT NOT NULL  UNIQUE , \"beatmapsetid\" INTEGER NOT NULL , \"beatmapid\" INTEGER NOT NULL , \"artist\" TEXT NOT NULL , \"title\" TEXT NOT NULL , \"difficulty\" TEXT NOT NULL , \"rank\" TEXT NOT NULL , \"status\" INTEGER NOT NULL  DEFAULT 0, \"lastplayed\" DATETIME NOT NULL  DEFAULT CURRENT_DATE, \"stars\" INTEGER NOT NULL, \"creator\" TEXT, \"backgroundpath\" TEXT, \"audiopath\" TEXT, \"audiopreviewtime\" INTEGER NOT NULL  DEFAULT 0, \"description\" TEXT, \"source\" TEXT, \"tags\" TEXT)";

                    dbCmd.CommandText = query;
                    dbCmd.ExecuteScalar(); // Execute scalar when inserting
                    dbConnection.Close();
                    Debug.Log("[CACHE] Beatmap Database Loaded/Generated!");
                }
            }
        }

        // Upon game launch, we'll want to load all of the beatmaps in the database to a List<CachedBeatmap>
        // These will be the maps that we will be available for song select.
        public static List<CachedBeatmap> LoadBeatmaps(Cfg userConfig)
        {
            List<CachedBeatmap> tempBeatmaps = new List<CachedBeatmap>();

            // First we'll want to find all beatmaps in the database.
            // Create db connection
            using (IDbConnection dbConnection = new SqliteConnection(s_connectionString))
            {
                // Open db connection
                dbConnection.Open();

                // Use connection to create an SQL Query we can execute
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
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
                                                                reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8),
                                                                reader.GetInt32(9), DateTime.Now, reader.GetFloat(11), reader.GetString(12), reader.GetString(13),
                                                                reader.GetString(14), reader.GetInt32(15), reader.GetString(16), reader.GetString(17), reader.GetString(18));

                            // Log that shit for now, because i already know some fucking thing is going to go wrong.
                            /*Debug.Log(String.Format("[CACHE] Beatmap Loaded: Dir: {0}, Path: {1}, BSID: {2}, BID: {3}, Artist: {4}, Title: {5}, Diff: {6}, Rank: {7}, Status: {8}, LP: {9}, Stars: {10}", 
													loadedBeatmap.Directory, loadedBeatmap.Path, loadedBeatmap.BeatmapSetID, loadedBeatmap.BeatmapID,
													loadedBeatmap.Artist, loadedBeatmap.Title, loadedBeatmap.Difficulty, loadedBeatmap.Rank, loadedBeatmap.Status,
													loadedBeatmap.LastPlayed, loadedBeatmap.Stars));*/

                            // Append the loaded beatmap to our List
                            tempBeatmaps.Add(loadedBeatmap);
                        }

                        // Close DB & Reader
                        dbConnection.Close();
                        reader.Close();
                    }
                }
            }

            // Run a check if the amount of tempBeatmaps from the DB is equivalent to the number of .qua files in the songs directory
            // If they aren't, we'll have to add/delete them to/from the database, and also append/truncate them to the tempBeatmaps list.
            // Also run a check if each beatmap in the database matches with a song in the file system.

            // This'll store the list of beatmaps that we'll be returning back to the user.
            // These will be all beatmaps that are playable. Any other "tempBeatmaps" variable is temporary.
            List<CachedBeatmap> finalLoadedBeatmaps = new List<CachedBeatmap>();
            try
            {
                // This is all of the .qua files in the current songs directory
                string[] quaFilesInDir = Directory.GetFiles(userConfig.SongDirectory, "*.qua", SearchOption.AllDirectories);

                if (tempBeatmaps.Count != quaFilesInDir.Length)
                {
                    Debug.LogWarning(String.Format("[CACHE] Incorrect # of Loaded Beatmaps vs. Qua Files ({0} vs {1})", tempBeatmaps.Count, quaFilesInDir.Length));
                }

                // Loop through all of the .qua files, and check if we have any missing maps.
                // For every .qua file, we'll look to see if the paths match, if not, the 
                // map will be considered as missing and will be added to the database,
                // + appended to the list.
                List<string> missingMaps = new List<string>();

                // Add the missing .qua files to the database, and add them to the tempBeatmaps list.
                if (missingMaps.Count > 0)
                    Debug.LogWarning("[CACHE] # of missing .qua files in database: " + missingMaps.Count);

                // Check if each qua file is already in the array. If it isn't, then add it.
                foreach (string quaFile in quaFilesInDir)
                {
                    bool foundMap = false;
                    for (int i = 0; i < tempBeatmaps.Count; i++)
                    {
                        if (quaFile == tempBeatmaps[i].Path)
                        {
                            foundMap = true;
                            break;
                        }
                    }

                    // If the map was found in the db, just check the next .qua file.
                    if (foundMap)
                        continue;

                    // If the entire loop completes, this .qua file must be missing.
                    missingMaps.Add(quaFile);

                    // Parse the qua file, get the necessary data, and add it to the database.
                    QuaFile qua = QuaParser.Parse(quaFile);

                    if (qua.IsValidQua)
                    {
                        // Convert the parsed qua file into a CachedBeatmap and add it to our tempBeatmaps.
                        string quaDir = Path.GetDirectoryName(quaFile);
                        string bgPath = quaDir + "/" + qua.BackgroundFile.Replace("\"", "");
                        string audioPath = quaDir + "/" + qua.AudioFile.Replace("\"", "");

                        if (Strings.IsNullOrEmptyOrWhiteSpace(qua.Description))
                        {
                            qua.Description = "This beatmap was converted from osu!mania.";
                        }

                        CachedBeatmap foundMissingMap = new CachedBeatmap(quaDir, quaFile, -1, -1, qua.Artist, qua.Title, qua.DifficultyName,
                                                                        "", 0, DateTime.Now, 0.0f, qua.Creator, bgPath, audioPath, qua.SongPreviewTime, qua.Description, qua.Source, qua.Tags);

                        // Add the missing map to our loaded map, when the beatmap is selected, we'll have to get the BeatmapSetID + BeatmapId
                        // + Status and update that, but ONLY when the map has been selected.										
                        tempBeatmaps.Add(foundMissingMap);

                        // Add the found missing map to the database.
                        AddToDatabase(foundMissingMap);
                    }
                    else
                    {
                        // Delete the .qua file from the file system.
                        File.Delete(quaFile);
                    }
                }

                // Since we should now have all of the missing qua files up to date, let's run a check for all of 
                // the maps in our database, that we don't have on the file system.
                foreach (CachedBeatmap mapInDb in tempBeatmaps)
                {
                    bool foundMapInDb = false;
                    for (int i = 0; i < quaFilesInDir.Length; i++)
                    {
                        if (mapInDb.Path == quaFilesInDir[i])
                        {
                            foundMapInDb = true;

                            // Add the found map to the final list of maps we'll be returning
                            finalLoadedBeatmaps.Add(mapInDb);
                            break;
                        }
                    }

                    // If the map was indeed fuond on the file system, check the next map.
                    if (foundMapInDb)
                        continue;

                    // Delete the map from the database.
                    DeleteFromDatabase(mapInDb);

                    Debug.LogWarning("[CACHE] Extra map: " + mapInDb.Path + " in database found, removed from loaded beatmaps & database!");
                }
            }
            catch (Exception err)
            {
                Debug.LogError("[CACHE] Unable to check songs directory for .qua files. Is the SongDirectory correct?" + err.ToString());
            }

            return finalLoadedBeatmaps;
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
                    string query = String.Format("INSERT INTO beatmaps(directory,path,beatmapsetid,beatmapid,artist,title,difficulty,rank,status,lastplayed,stars,creator,backgroundpath,audiopath,audiopreviewtime,description,source,tags) " +
                                    "VALUES(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\", \"{10}\", \"{11}\", \"{12}\", \"{13}\", \"{14}\", \"{15}\", \"{16}\", \"{17}\")",
                                    cachedMap.Directory, cachedMap.Path, cachedMap.BeatmapSetID, cachedMap.BeatmapID,
                                    cachedMap.Artist.Replace("\"", String.Empty), cachedMap.Title.Replace("\"", String.Empty), cachedMap.Difficulty.Replace("\"", String.Empty), cachedMap.Rank.Replace("\"", String.Empty), cachedMap.Status,
                                    cachedMap.LastPlayed, cachedMap.Stars, cachedMap.Creator.Replace("\"", String.Empty), cachedMap.BackgroundPath, cachedMap.AudioPath, cachedMap.AudioPreviewTime,
                                    cachedMap.Description.Replace("\"", String.Empty), cachedMap.Source.Replace("\"", String.Empty), cachedMap.Tags.Replace("\"", String.Empty));

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

        // Responsible for converting a .qua file to a CachedBeatmap
        public static CachedBeatmap ConvertQuaToCached(string fileName)
        {
            QuaFile qua = QuaParser.Parse(fileName);
            
            if (qua.IsValidQua)
            {
                // Convert the parsed qua file into a CachedBeatmap and add it to our tempBeatmaps.
                string quaDir = Path.GetDirectoryName(fileName);
                string bgPath = quaDir + "/" + qua.BackgroundFile.Replace("\"", "");
                string audioPath = quaDir + "/" + qua.AudioFile.Replace("\"", "");

                if (Strings.IsNullOrEmptyOrWhiteSpace(qua.Description))
                {
                    qua.Description = "This beatmap was converted from osu!mania.";
                }

                CachedBeatmap foundMissingMap = new CachedBeatmap(quaDir, fileName, -1, -1, qua.Artist, qua.Title, qua.DifficultyName,
                                                                "", 0, DateTime.Now, 0.0f, qua.Creator, bgPath, audioPath, qua.SongPreviewTime, qua.Description, qua.Source, qua.Tags);                
            }

            return new CachedBeatmap(false);
        }

        // Responsible for refreshing all of the directories in the queue.
        // This will CRUD maps as expected.
		public static void RefreshDirectory(Cfg userConfig)
		{
			if (GameStateManager.SongDirectoryChangeQueue.Count > 0 && Input.GetKeyDown(KeyCode.F5))
			{
				Debug.Log("[BEATMAP CACHE] Refreshing " + GameStateManager.SongDirectoryChangeQueue.Count + " beatmap directories");

                // This'll store the list of directories we'll be deleting from the Queue, after it's done
                List<string> dirsToDelete = new List<string>();

				foreach (string directory in GameStateManager.SongDirectoryChangeQueue)
				{

                    // Get the full current directory of the beatmap
                    string currentDir = userConfig.SongDirectory + directory;

                    // Get all the files in the directory
                    string[] files = Directory.GetFiles(currentDir, "*.qua");

                    // Store a list of all the files in the db
                    List<string> filesInDb = new List<string>();

                    using (IDbConnection dbConnection = new SqliteConnection(s_connectionString))
                    {
                        // Open db connection
                        dbConnection.Open();

                        // Use connection to create an SQL Query we can execute
                        using (IDbCommand dbCmd = dbConnection.CreateCommand())
                        {
                            Debug.Log("CURRENT DIR: " + currentDir);
                            string query = "SELECT * FROM beatmaps WHERE directory='" + currentDir + "'";

                            dbCmd.CommandText = query;

                            // To be able to read the database data
                            using (IDataReader reader = dbCmd.ExecuteReader())
                            {
                                
                                while (reader.Read())
                                {
                                    filesInDb.Add(reader.GetString(2));
                                }

                                // Close DB & Reader
                                dbConnection.Close();
                                reader.Close();
                            }
                        }
                    }

                    // Loop through all the files in the database
                    foreach(string file in filesInDb)
                    {
                        // If the file is in both the directory, and the db, then reparse the beatmap
                        // and update any relavant data.
                        if (File.Exists(file) && filesInDb.Contains(file))
                        {
                            CachedBeatmap mapToUpdate = FindCachedMap(file);

                            // If the map is in the database, and is valid, this means it needs to be updated.
                            if (mapToUpdate.Valid)
                            {
                                // So, we'll remove it from the database, and add it again.
                                DeleteFromDatabase(mapToUpdate);
                                AddToDatabase(mapToUpdate);
                            }
                        }

                        // If the file does not exist on the file system, but still exists in the database,
                        // Delete it.
                        if (!File.Exists(file))
                        {                            
                            // Find that particular beatmap in a cached map
                            CachedBeatmap mapToDelete = FindCachedMap(file);

                            if (mapToDelete.Valid)
                            {
                                Debug.Log("[BEATMAP CACHE] Removing beatmap: " + file + " from the database as it does not exist anymore.");

                                // Delete it from the database
                                DeleteFromDatabase(mapToDelete);

                                // Find the map's directory, and check if that directory is null or empty.
                                // If it isn't, that must mean the MapDirectory is valid and that particular beatmap
                                // can be removed!
                                MapDirectory beatmapMapDirectory = FindMapDirectory(mapToDelete);
                                if (!String.IsNullOrEmpty(beatmapMapDirectory.Directory))
                                {
                                    for (int i = 0; i < GameStateManager.MapDirectories.Count; i++)
                                    {
                                        if (GameStateManager.MapDirectories[i].Beatmaps.Contains(mapToDelete))
                                        {
                                            GameStateManager.MapDirectories[i].Beatmaps.Remove(mapToDelete);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    dirsToDelete.Add(directory);				
				}

                // Remove all missing directories from the queue.
                foreach (string dir in dirsToDelete)
                {
                    GameStateManager.SongDirectoryChangeQueue.RemoveAll(queueItem => queueItem == dir);
                }

                Debug.Log("[BEATMAP CACHE] Change Queue Updated with: " + GameStateManager.SongDirectoryChangeQueue.Count + " maps in it.");
			}
		}

        // Finds a cached beatmap 
        public static CachedBeatmap FindCachedMap(string fileName)
        {
            for (int i = 0; i < GameStateManager.LoadedBeatmaps.Count; i++)
            {
                if (GameStateManager.LoadedBeatmaps[i].Path == fileName)
                {
                    return GameStateManager.LoadedBeatmaps[i];
                }
            }

            return new CachedBeatmap(false);
        }

        // Finds the MapDirectory object the beatmap is stored in
        public static MapDirectory FindMapDirectory(CachedBeatmap map)
        {
            for (int i = 0; i < GameStateManager.MapDirectories.Count; i++)
            {
                if (GameStateManager.MapDirectories[i].Beatmaps.Contains(map))
                {
                    return GameStateManager.MapDirectories[i];
                }
            }

            return new MapDirectory();
        }
    }
}

