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
	public class BeatmapWatcher
	{
        /// <summary>
        /// Watches the Songs directory for any changes, and reports an event.
        /// </summary>
        /// <param name="userConfig">The user configuration object</param>
		public static void Watch(Cfg userConfig)
		{
			FileSystemWatcher watcher = new FileSystemWatcher(userConfig.SongDirectory);

			watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
            | NotifyFilters.FileName |NotifyFilters.DirectoryName;

			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Created += new FileSystemEventHandler(OnChanged);
			watcher.Deleted += new FileSystemEventHandler(OnChanged);
			watcher.EnableRaisingEvents = true;
		}

        /// <summary>
        /// Adds the changed directory to the queue of changes. 
        /// It wont be directory changed until the user presses F5.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="e">All the other cool stuff we get with the change</param>
		private static void OnChanged(object source, FileSystemEventArgs e)
		{
			FileInfo file = new FileInfo(e.FullPath);
			WatcherChangeTypes wct = e.ChangeType;

			Debug.LogWarning("[BEATMAP WATCHER] Detected Change Type: " + wct.ToString() + " on Directory: " + file.Name);

			// Add the song directory to the queue.
			if (!GameStateManager.SongDirectoryChangeQueue.Contains(file.Name))
			{
				GameStateManager.SongDirectoryChangeQueue.Add(file.Name);
				Debug.Log("Press F5 to refresh beatmaps.");
				Debug.Log(GameStateManager.SongDirectoryChangeQueue.Count);
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
