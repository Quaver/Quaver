﻿
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Quaver.Cache
{
    public class CachedBeatmap
    {
        public bool Valid = false;
        public string Directory;
        public string Path;
        public int BeatmapSetID;
        public int BeatmapID;
        public string Artist;
        public string Title;
        public string Difficulty;
        public string Rank;
        public int Status;
        public DateTime LastPlayed;
        public float Stars;
        public string Creator;
        public string BackgroundPath;
        public string AudioPath;
        public int AudioPreviewTime;
        public string Description;
        public string Source;
        public string Tags;

        // Set all the cached beatmap values in the constructor
        public CachedBeatmap(string directory, string path, int beatmapSetID, int beatmapID, string artist,
                            string title, string difficulty, string rank, int status, DateTime lastPlayed, float stars,
                            string creator, string bgPath, string audioPath, int audioPreviewTime, string description,
                            string source, string tags)
        {
            this.Directory = directory;
            this.Path = path;
            this.BeatmapSetID = beatmapSetID;
            this.BeatmapID = beatmapID;
            this.Artist = artist;
            this.Title = title;
            this.Difficulty = difficulty;
            this.Rank = rank;
            this.Status = status;
            this.LastPlayed = lastPlayed;
            this.Stars = stars;
            this.Creator = creator;
            this.BackgroundPath = bgPath;
            this.AudioPath = audioPath;
            this.AudioPreviewTime = audioPreviewTime;
            this.Description = description;
            this.Source = source;
            this.Tags = tags;
            this.Valid = true;
        }

        public CachedBeatmap(bool valid)
        {
            // This'll hold an invalid map
            this.Valid = false;
        }
    }
}

