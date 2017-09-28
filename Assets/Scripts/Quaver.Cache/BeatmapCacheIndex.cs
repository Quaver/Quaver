
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
        // Responsible for loading all of the playable beatmaps and creating the List<CachedBeatmap>
        public static List<CachedBeatmap> LoadMaps(Cfg userConfig)
        {
            // First, we'll want to select all of the maps from the database and add them to a temporary List<CachedBeatmap>
            List<CachedBeatmap> temporaryMaps = new List<CachedBeatmap>();

            // Create the database connection and create a CachedBeatmap object from them
            // and add them to our list
            using (IDbConnection dbConnection = new SqliteConnection(BeatmapDBInterface.s_connectionString))
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
                    CachedBeatmap newCachedMap = BeatmapCacheUtilities.ConvertQuaToCached(quaFilesInDirectory[i]);

                    if (newCachedMap.Valid)
                    {
                        BeatmapDBInterface.AddToDatabase(newCachedMap);
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
                    BeatmapDBInterface.DeleteFromDatabase(temporaryBeatmaps[i]);
                    Debug.LogWarning("[CACHE] Extra map: " + temporaryBeatmaps[i].Path + " in database found, removed from loaded beatmaps & database!");
                }

                return syncedMaps;

            } catch (Exception e)
            {
                Debug.LogException(e);
                return temporaryBeatmaps;
            }
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

                        using (IDbConnection dbConnection = new SqliteConnection(BeatmapDBInterface.s_connectionString))
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
                                CachedBeatmap mapToUpdate = BeatmapCacheUtilities.FindCachedMap(file);

                                // If the map is in the database, and is valid, this means it needs to be updated.
                                if (mapToUpdate.Valid)
                                {
                                    // So, we'll remove it from the database, and add it again.
                                    BeatmapDBInterface.DeleteFromDatabase(mapToUpdate);
                                    BeatmapDBInterface.AddToDatabase(mapToUpdate);
                                }
                            }

                            // If the file does not exist on the file system, but still exists in the database,
                            // Delete it.
                            if (!File.Exists(file))
                            {                            
                                // Find that particular beatmap in a cached map
                                CachedBeatmap mapToDelete = BeatmapCacheUtilities.FindCachedMap(file);

                                if (mapToDelete.Valid)
                                {
                                    Debug.Log("[BEATMAP CACHE] Removing beatmap: " + file + " from the database as it does not exist anymore.");

                                    // Delete it from the database
                                    BeatmapDBInterface.DeleteFromDatabase(mapToDelete);

                                    // Find the map's directory, and check if that directory is null or empty.
                                    // If it isn't, that must mean the MapDirectory is valid and that particular beatmap
                                    // can be removed!
                                    MapDirectory beatmapMapDirectory = BeatmapCacheUtilities.FindMapDirectory(mapToDelete);
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
                                CachedBeatmap newMap = BeatmapCacheUtilities.ConvertQuaToCached(file);

                                if (newMap.Valid)
                                {
                                    // Add Map To DB
                                    BeatmapDBInterface.AddToDatabase(newMap);

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
                    using (IDbConnection dbConnection = new SqliteConnection(BeatmapDBInterface.s_connectionString))
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
    }
}

