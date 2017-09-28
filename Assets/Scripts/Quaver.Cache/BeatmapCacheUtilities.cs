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
	public class BeatmapCacheUtilities
	{
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
                                        qua.Description, qua.Source, qua.Tags, BeatmapCacheUtilities.FindCommonBPM(qua), 
                                        BeatmapCacheUtilities.FindSongLength(qua));                
            }

            return new CachedBeatmap(false);
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