
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Quaver.Cache
{
    /// <summary>
    /// This class holds a beatmap that is cached in our database.
    /// </summary>
    public class CachedBeatmap
    {
        /// <summary>
        /// The validity of the CachedBeatmap
        /// </summary>
        public bool Valid = false;

        /// <summary>
        /// The directory of the beatmap
        /// </summary>
        public string Directory;

        /// <summary>
        /// The absolute path of the beatmap
        /// </summary>
        public string Path;

        /// <summary>
        /// The beatmap set id of the beatmap.
        /// </summary>
        public int BeatmapSetID;

        /// <summary>
        /// The beatmap Id of the beatmap.
        /// </summary>
        public int BeatmapID;

        /// <summary>
        /// The artist of the map.
        /// </summary>
        public string Artist;

        /// <summary>
        /// The title of the map.
        /// </summary>
        public string Title;

        /// <summary>
        /// The difficulty name of the map.
        /// </summary>
        public string Difficulty;

        /// <summary>
        /// The highest achieved rank on the map.
        /// </summary>
        public string Rank;

        /// <summary>
        /// The ranked status of the map.
        /// </summary>
        public int Status;

        /// <summary>
        /// The date time of the last time the user has played the map.
        /// </summary>
        public DateTime LastPlayed;

        /// <summary>
        /// The amount of stars the map has.
        /// </summary>
        public float Stars;

        /// <summary>
        /// The creator of the map.
        /// </summary>
        public string Creator;

        /// <summary>
        ///  The path of the background
        /// </summary>
        public string BackgroundPath;

        /// <summary>
        /// The path of the audio file.
        /// </summary>
        public string AudioPath;

        /// <summary>
        /// The preview time of the audio
        /// </summary>
        public int AudioPreviewTime;

        /// <summary>
        /// The description of the map.
        /// </summary>
        public string Description;

        /// <summary>
        /// The source of the song.
        /// </summary>
        public string Source;

        /// <summary>
        /// Specific tags for the song/map
        /// </summary>
        public string Tags;

        /// <summary>
        /// The most common BPM of the map.
        /// </summary>
        public int BPM;

        /// <summary>
        /// The length of the map.
        /// </summary>
        public int SongLength;

        // Set all the cached beatmap values in the constructor
        public CachedBeatmap(string directory, string path, int beatmapSetID, int beatmapID, string artist,
                            string title, string difficulty, string rank, int status, DateTime lastPlayed, float stars,
                            string creator, string bgPath, string audioPath, int audioPreviewTime, string description,
                            string source, string tags, int bpm, int songLength)
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
            this.BPM = bpm;
            this.SongLength = songLength;
            this.Valid = true;
        }

        public CachedBeatmap(bool valid)
        {
            // This'll hold an invalid map
            this.Valid = false;
        }
    }
}

