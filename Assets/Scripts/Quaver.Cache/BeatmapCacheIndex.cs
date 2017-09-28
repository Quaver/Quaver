﻿
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

        // Responsible for loading all of the playable beatmaps and creating the List<CachedBeatmap>
        public static List<CachedBeatmap> LoadMaps(Cfg userConfig)
        {
            // First, we'll want to select all of the maps from the database and add them to a temporary List<CachedBeatmap>
            List<CachedBeatmap> temporaryMaps = new List<CachedBeatmap>();

            // Create the database connection and create a CachedBeatmap object from them
            // and add them to our list
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
                            CachedBeatmap loadedBeatmap = new CachedBeatmap(
                            reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetInt32(4),
                            reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8),
                            reader.GetInt32(9), DateTime.Now, reader.GetFloat(11), reader.GetString(12), reader.GetString(13),
                            reader.GetString(14), reader.GetInt32(15), reader.GetString(16), reader.GetString(17), reader.GetString(18),
                            reader.GetInt32(19), reader.GetInt32(20));

                            // Append the loaded beatmap to our List
                            temporaryMaps.Add(loadedBeatmap);
                        }

                        // Close DB & Reader
                        dbConnection.Close();
                        reader.Close();
                    }
                }

                SyncMissingBeatmaps(temporaryMaps, userConfig);
                return SyncMissingQua(temporaryMaps, userConfig);
            }
           
        }

        // Responsible for checking the file system and matching each .qua file to a row in the database.
        // If there are more db rows, than .qua files, it'll get rid of the extra ones, and vice versa.
        private static void SyncMissingBeatmaps(List<CachedBeatmap> temporaryBeatmaps, Cfg userConfig)
        {
            try
            {
                // Find all of the .qua files in the directory
                string[] quaFilesInDirectory = Directory.GetFiles(userConfig.SongDirectory, "*.qua", SearchOption.AllDirectories);

                // If the number of loaded beatmaps and qua files don't match, we'll want to warn the user.
                if (temporaryBeatmaps.Count != quaFilesInDirectory.Length)
                {
                    Debug.LogWarning("[BEATMAP CACHE] Incorrect number of loaded beatmaps vs. Qua Files: " + 
                    temporaryBeatmaps.Count + " vs. " + quaFilesInDirectory.Length);
                }

                // Here, we'll want to find all of the .qua files that are missing from the database and add them.
                for (int i = 0; i < quaFilesInDirectory.Length; i++)
                {   
                    bool foundMap = false;

                    for (int j = 0; j < temporaryBeatmaps.Count; j++)
                    {
                        if (quaFilesInDirectory[i] == temporaryBeatmaps[j].Path)
                        {
                            foundMap = true;
                            break;
                        }
                    }

                    // If the map ended up being found in the database, we'll want to move onto the next one
                    if (foundMap) continue;

                    // Otherwise, we'll want to parse the map and add it to the DB.
                    CachedBeatmap newCachedMap = ConvertQuaToCached(quaFilesInDirectory[i]);

                    if (newCachedMap.Valid)
                    {
                        AddToDatabase(newCachedMap);
                        temporaryBeatmaps.Add(newCachedMap);
                    }
                    else 
                    {
                        File.Delete(quaFilesInDirectory[i]);
                    }                
                }
            } catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        // Responsible for syncing maps that are in the database, but don't have a valid .qua file attached to them.
        private static List<CachedBeatmap> SyncMissingQua(List<CachedBeatmap> temporaryBeatmaps, Cfg userConfig)
        {
            try 
            {
                string[] quaFilesInDirectory = Directory.GetFiles(userConfig.SongDirectory, "*.qua", SearchOption.AllDirectories);
                List<CachedBeatmap> syncedMaps = new List<CachedBeatmap>();

                // Attempt to find a match of the .qua file and beatmap.
                for (int i = 0; i < temporaryBeatmaps.Count; i++)
                {
                    bool matchedQuaWithDb = false;
                    for (int j = 0; j < quaFilesInDirectory.Length; j++)
                    {
                        // If the .qua was found then we'll want to move onto the next one.
                        if (temporaryBeatmaps[i].Path == quaFilesInDirectory[j])
                        {
                            matchedQuaWithDb = true;
                            syncedMaps.Add(temporaryBeatmaps[i]);
                            break;
                        }
                    }

                    // If there was a match, continue onto the next beatmap.
                    if (matchedQuaWithDb) continue;

                    // If there wasn't we'll want to delete it from the database.
                    DeleteFromDatabase(temporaryBeatmaps[i]);
                    Debug.LogWarning("[CACHE] Extra map: " + temporaryBeatmaps[i].Path + " in database found, removed from loaded beatmaps & database!");
                }

                return syncedMaps;

            } catch (Exception e)
            {
                Debug.LogException(e);
                return temporaryBeatmaps;
            }
        }

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

        // Responsible for converting a .qua file to a CachedBeatmap
        public static CachedBeatmap ConvertQuaToCached(string fileName)
        {
            QuaFile qua = QuaParser.Parse(fileName, false);
            
            if (qua.IsValidQua)
            {
                // Convert the parsed qua file into a CachedBeatmap and add it to our tempBeatmaps.
                string quaDir = Path.GetDirectoryName(fileName);
                string bgPath = quaDir + "/" + qua.BackgroundFile.Replace("\"", "");
                string audioPath = quaDir + "/" + qua.AudioFile.Replace("\"", "");

                if (Strings.IsNullOrEmptyOrWhiteSpace(qua.Description))
                {
                    qua.Description = "No Description.";
                }

                return new CachedBeatmap(quaDir, fileName, -1, -1, qua.Artist, qua.Title, qua.DifficultyName,
                                        "", 0, DateTime.Now, 0.0f, qua.Creator, bgPath, audioPath, qua.SongPreviewTime, 
                                        qua.Description, qua.Source, qua.Tags, FindCommonBPM(qua), FindSongLength(qua));                
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
                string currentDir = "";

                try 
                {
                    foreach (string directory in GameStateManager.SongDirectoryChangeQueue)
                    {

                        // Get the full current directory of the beatmap
                        currentDir = userConfig.SongDirectory + directory;

                        List<string> files = new List<string>();

                        // Try to get all the files in the directory
                        files = Directory.GetFiles(currentDir, "*.qua").ToList();

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

                        // Now loop through all the files, and find the ones that arent in the database
                        // but are actually on the file system, they are considered new maps and we want
                        // to add them.
                        foreach(var file in files)
                        {
                            if (File.Exists(file) && !filesInDb.Contains(file))
                            {                        
                                // If the map isn't valid, we need to go ahead and parse that file, and add it
                                // manually the to the database using SQL.
                                CachedBeatmap newMap = ConvertQuaToCached(file);

                                if (newMap.Valid)
                                {
                                    // Add Map To DB
                                    AddToDatabase(newMap);

                                    // Add map to loaded beatmaps
                                    GameStateManager.LoadedBeatmaps.Add(newMap);

                                    // Find the map directory that the beatmap is supposed to live in. If it exists,
                                    // Add the beatmap to that directory.
                                    bool foundBeatmapDirectory = false;
                                    for (int i = 0; i < GameStateManager.MapDirectories.Count; i++)
                                    {
                                        if (GameStateManager.MapDirectories[i].Directory == Path.GetDirectoryName(file))
                                        {
                                            Debug.Log("[BEATMAP CACHE] Found the correct MapDirectory for file: " + file);
                                            // Add the new map to the correct directory.
                                            GameStateManager.MapDirectories[i].Beatmaps.Add(newMap);

                                            foundBeatmapDirectory = true;
                                        }
                                    }

                                    // If we still haven't found the correct beatmap directory, then we'll create a new one.
                                    if (!foundBeatmapDirectory)
                                    {
                                        // Create a new MapDirectory and add the correct data.
                                        MapDirectory newDir = new MapDirectory();
                                        newDir.Directory = Path.GetDirectoryName(file);
                                        
                                        newDir.Beatmaps = new List<CachedBeatmap>();
                                        newDir.Beatmaps.Add(newMap);

                                        // Add the new map directory to the current list!
                                        GameStateManager.MapDirectories.Add(newDir);

                                        Debug.Log("[BEATMAP CACHE] Did not find the correct MapDirectory, so creating one for file: " + file);
                                    }

                                    Debug.Log("[BEATMAP CACHE] Added new unknown file to database: " + file);
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
                catch (DirectoryNotFoundException e)
                {
                    Debug.LogException(e);

                    // If the directory isn't found then we want to remove every single .qua file from the database,
                    // delete the MapDirectory, & remove them from the list of loadedbeatmaps
                    using (IDbConnection dbConnection = new SqliteConnection(s_connectionString))
                    {
                        // Open db connection
                        dbConnection.Open();

                        // Use connection to create an SQL Query we can execute
                        using (IDbCommand dbCmd = dbConnection.CreateCommand())
                        {
                            string query = "DELETE FROM beatmaps WHERE directory=\"" + currentDir + "\"";

                            dbCmd.CommandText = query;

                            dbCmd.ExecuteScalar(); // Execute scalar when inserting

                            dbConnection.Close();
                        }
                    } 

                    dirsToDelete.Add(currentDir);

                    // Remove all missing directories from the queue.
                    foreach (string dir in dirsToDelete)
                    {
                        MapDirectory mapDirToDelete = new MapDirectory();

                        // Find the MapDirectory that matches the one of the current iteration
                        foreach (MapDirectory mapDir in GameStateManager.MapDirectories)
                        {
                            if (mapDir.Directory == dir)
                            {
                                mapDirToDelete = mapDir;
                                break;
                            }
                        }

                        if (GameStateManager.MapDirectories.Contains(mapDirToDelete))
                        {
                            GameStateManager.MapDirectories.Remove(mapDirToDelete);
                            Debug.Log("[BEATMAP CACHE] Map Directory: " + currentDir + " was deleted, and has been removed from the cache.");
                        }                        
                    } 

                    GameStateManager.SongDirectoryChangeQueue.RemoveAll(queueItem => currentDir.Contains(queueItem) || queueItem.Contains(".meta"));
                    foreach (string queue in GameStateManager.SongDirectoryChangeQueue)
                    {
                        Debug.Log(queue + " | " + currentDir);
                    
                    }
                    Debug.Log("[BEATMAP CACHE] Change Queue Updated with: " + GameStateManager.SongDirectoryChangeQueue.Count + " maps in it.");                  
                }

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

        // Finds the most common BPM in a qua file.
        public static int FindCommonBPM(QuaFile qua)
        {
            var commonBPM = (int)qua.TimingPoints.GroupBy(i => i.BPM).OrderByDescending(grp => grp.Count())
                .Select(grp=>grp.Key).First();

            return commonBPM;            
        }

        // Finds the length of the beatmap.
        public static int FindSongLength(QuaFile qua)
        {
            // This'll check for either the end time of the last object if it's greater than 0, or the start time.
            HitObject lastHitObject = qua.HitObjects[qua.HitObjects.Count - 1];

            if (lastHitObject.EndTime > 0)
            {
                return lastHitObject.EndTime;
            }

            return lastHitObject.StartTime;
        }
    }
}

