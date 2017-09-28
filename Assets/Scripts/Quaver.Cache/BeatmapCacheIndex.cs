
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
    }
}

