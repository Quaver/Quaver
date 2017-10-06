using System;
using System.IO;
using System.Security.Cryptography;
using Quaver.QuaFile;

namespace Quaver.Beatmaps
{
    internal class Beatmap
    {   /// <summary>
        ///     The MD5 Of the file.
        /// </summary>
        public string Md5Checksum { get; set; }

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
        public decimal Bpm { get; set; }

        /// <summary>
        ///     The beatmap's length (Time of the last hit object)
        /// </summary>
        public int SongLength { get; set; }

        /// <summary>
        ///     Responsible for converting a Qua object, to a Beatmap object
        ///     a Beatmap object is one that is stored in the db.
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal Beatmap ConvertQuaToBeatmap(Qua qua, string path)
        {

            return new Beatmap
            {
                Md5Checksum = GetMd5Checksum(path),
                IsValidBeatmap = true,
                Path = path,
                Artist = qua.Artist,
                Title = qua.Title,
                AudioPath = qua.AudioFile,
                AudioPreviewTime = qua.SongPreviewTime,
                BackgroundPath = qua.BackgroundFile,
                BeatmapId = qua.MapId,
                BeatmapSetId = qua.MapSetId,
                Bpm = Qua.FindCommonBpm(qua),
                Creator = qua.Creator,
                DifficultyName = qua.DifficultyName,
                Source = qua.Source,
                Tags = qua.Tags,
                SongLength = Qua.FindSongLength(qua),
            };
        }

        /// <summary>
        ///     Gets the Md5 Checksum of a file, more specifically a .qua file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetMd5Checksum(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
                }
            }
        }
    }
}