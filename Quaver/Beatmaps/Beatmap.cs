using System;
using Quaver.QuaFile;

namespace Quaver.Beatmaps
{
    internal class Beatmap
    {
        /// <summary>
        ///     Is the beatmap valid and able to be played?
        /// </summary>
        public bool IsValidBeatmap { get; set; }

        /// <summary>
        ///     The absolute path of the .qua file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     The beatmap set id of the map.
        /// </summary>
        public int BeatmapSetId { get; set; }

        /// <summary>
        ///     The beatmap id of the map.
        /// </summary>
        public int BeatmapId { get; set; }

        /// <summary>
        ///     The song's artist
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        ///     The song's title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The difficulty name of the beatmap
        /// </summary>
        public string DifficultyName { get; set; }

        /// <summary>
        ///     The highest rank that the player has gotten on the map.
        /// </summary>
        public string HighestRank { get; set; }

        /// <summary>
        ///     The ranked status of the map.
        /// </summary>
        public int RankedStatus { get; set; }

        /// <summary>
        ///     The last time the user has played the map.
        /// </summary>
        public DateTime LastPlayed { get; set; } =
            new DateTime(0001, 1, 1, 00, 00, 00); // 01/01/0001 00:00:00 - If never played

        /// <summary>
        ///     The difficulty rating of the beatmap.
        /// </summary>
        public float DifficultyRating { get; set; }

        /// <summary>
        ///     The creator of the map.
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        ///     The absolute path of the beatmap's background.
        /// </summary>
        public string BackgroundPath { get; set; }

        /// <summary>
        ///     The absolute path of the beatmap's audio.
        /// </summary>
        public string AudioPath { get; set; }

        /// <summary>
        ///     The audio preview time of the beatmap
        /// </summary>
        public int AudioPreviewTime { get; set; }

        /// <summary>
        ///     The description of the beatmap
        /// </summary>
        public string Description { get; set; } = "No Description";

        /// <summary>
        ///     The source (album/mixtape/etc) of the beatmap
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Tags for the beatmap
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        ///     The most common bpm for the beatmap
        /// </summary>
        public int Bpm { get; set; }

        /// <summary>
        ///     The beatmap's length (Time of the last hit object)
        /// </summary>
        public int SongLength { get; set; }

        internal Beatmap ConvertQuaToBeatmap(Qua qua, string path)
        {
            return new Beatmap
            {
                IsValidBeatmap = true,
                Path = path,
                Artist = qua.Artist,
                Title = qua.Title,
                AudioPath = qua.AudioFile,
                AudioPreviewTime = qua.SongPreviewTime,
                BackgroundPath = qua.BackgroundFile,
                BeatmapId = qua.MapId,
                BeatmapSetId = qua.MapSetId,
                Bpm = 2,
                Creator = qua.Creator,
                DifficultyName = qua.DifficultyName,
                Source = qua.Source,
                Tags = qua.Tags
            };
        }
    }
}