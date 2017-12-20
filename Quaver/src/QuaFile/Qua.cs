using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Logging;
using Quaver.Peppy;
using Quaver.Enums;
using Newtonsoft.Json;

namespace Quaver.QuaFile
{
    public class Qua
    {
        /// <summary>
        ///     The format version of the .qua file, so we can keep track of how
        ///     to deal with things depending on the version
        /// </summary>
        public int FormatVersion { get; set; }

        /// <summary>
        ///     The name of the audio file
        /// </summary>
        public string AudioFile { get; set; }

        /// <summary>
        ///     Time in milliseconds of the song where the preview starts
        /// </summary>
        public int SongPreviewTime { get; set; }

        /// <summary>
        ///     The name of the background file
        /// </summary>
        public string BackgroundFile { get; set; }

        /// <summary>
        ///     The unique Map Identifier (-1 if not submitted)
        /// </summary>
        public int MapId { get; set; }

        /// <summary>
        ///     The unique Map Set identifier (-1 if not submitted)
        /// </summary>
        public int MapSetId { get; set; }

        /// <summary>
        ///     The game mode for this map
        /// </summary>
        public GameModes Mode { get; set; }

        /// <summary>
        ///     The title of the song
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The artist of the song
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        ///     The source of the song (album, mixtape, etc.)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Any tags that could be used to help find the song.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        ///     The creator of the map
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        ///     The difficulty name of the map.
        /// </summary>
        public string DifficultyName { get; set; }

        /// <summary>
        ///     A description about this map.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     TODO: Remove.
        /// </summary>
        public int Judge { get; set; } = 0;

        /// <summary>
        ///     TimingPoint .qua data
        /// </summary>
        public List<TimingPointInfo> TimingPoints { get; set; }
        /// <summary>
        ///     Slider Velocity .qua data
        /// </summary>
        public List<SliderVelocityInfo> SliderVelocities { get; set; }

        /// <summary>
        ///     HitObject .qua data
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; set; }

        /// <summary>
        ///     Is the Qua actually valid?
        /// </summary>
        [JsonIgnore]
        public bool IsValidQua { get; set; }

        /// <summary>
        ///     Takes in a path to a .qua file and attempts to parse it.
        ///     Will throw an error if unable to be parsed.
        /// </summary>
        /// <param name="path"></param>
        public static Qua Parse(string path)
        {
            var qua = new Qua();

            using (var file = File.OpenText(path))
            {
                var serializer = new JsonSerializer();
                qua = (Qua)serializer.Deserialize(file, typeof(Qua));
            }

            // Check the Qua object's validity.
            qua.IsValidQua = CheckQuaValidity(qua);

            // Try to sort the Qua before returning.
            qua.Sort();

            return qua;
        }

        /// <summary>
        ///     Serializes the Qua object to a file.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            // Sort the object before saving.
            Sort();

            // Save
            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer() { Formatting = Formatting.Indented };
                serializer.Serialize(file, this);
            }
        }

        /// <summary>
        /// Finds the most common BPM in a Qua object.
        /// </summary>
        /// <param name="qua"></param>
        /// <returns></returns>
        internal static decimal FindCommonBpm(Qua qua)
        {
            if (qua.TimingPoints.Count == 0)
                return 0;

            return Math.Round((decimal)qua.TimingPoints.GroupBy(i => i.Bpm).OrderByDescending(grp => grp.Count())
                .Select(grp => grp.Key).First(), 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        ///     Finds the length of the beatmap
        ///     (Time of the last hit object.)
        /// </summary>
        /// <param name="qua"></param>
        /// <returns></returns>
        internal static int FindSongLength(Qua qua)
        {
            if (qua.HitObjects.Count == 0)
                return 0;

            //Get song end by last note
            var LastNoteEnd = 0;
            for (var i = qua.HitObjects.Count - 1; i > 0; i--)
            {
                var ho = qua.HitObjects[i];
                if (ho.EndTime > LastNoteEnd)
                    LastNoteEnd = ho.EndTime;
                else if (ho.StartTime > LastNoteEnd)
                    LastNoteEnd = ho.StartTime;
            }

            return LastNoteEnd;
        }

        /// <summary>
        ///     Checks a Qua object's validity.
        /// </summary>
        /// <param name="qua"></param>
        /// <returns></returns>
        private static bool CheckQuaValidity(Qua qua)
        {
            // If there aren't any HitObjects
            if (qua.HitObjects.Count == 0)
                return false;

            // If there aren't any TimingPoints
            if (qua.TimingPoints.Count == 0)
                return false;

            // Check if the mode is actually valid
            if (!Enum.IsDefined(typeof(GameModes), qua.Mode))
                return false;

            return true;
        }

        /// <summary>
        ///     Does some sorting of the Qua
        /// </summary>
        private void Sort()
        {
            try
            {
                HitObjects = HitObjects.OrderBy(x => x.StartTime).ToList();
                TimingPoints = TimingPoints.OrderBy(x => x.StartTime).ToList();
                SliderVelocities = SliderVelocities.OrderBy(x => x.StartTime).ToList();
            }
            catch (Exception e)
            {
                IsValidQua = false;
            }
        }

        /// <summary>
        ///     Converts an .osu file into a Qua object
        /// </summary>
        /// <param name="osu"></param>
        /// <returns></returns>
        public static Qua ConvertOsuBeatmap(PeppyBeatmap osu)
        {
            // Init Qua with general information
            var qua = new Qua()
            {
                FormatVersion = 1,
                AudioFile = osu.AudioFilename,
                SongPreviewTime = osu.PreviewTime,
                BackgroundFile = osu.Background,
                MapId = -1,
                MapSetId = -1,
                Title = osu.Title,
                Artist = osu.Artist,
                Source = osu.Source,
                Tags = osu.Tags,
                Creator = osu.Creator,
                DifficultyName = osu.Version,
                Description = $"This is a Quaver converted version of {osu.Creator}'s map."
            };

            // Get the correct game mode based on the amount of keys the map has.
            switch (osu.KeyCount)
            {
                case 4:
                    qua.Mode = GameModes.Keys4;
                    break;
                case 7:
                    qua.Mode = GameModes.Keys7;
                    break;
                default:
                    qua.Mode = (GameModes)(-1);
                    break;
            }

            // Initialize lists
            qua.TimingPoints = new List<TimingPointInfo>();
            qua.SliderVelocities = new List<SliderVelocityInfo>();
            qua.HitObjects = new List<HitObjectInfo>();

            // Get Timing Info
            foreach (var tp in osu.TimingPoints)
                if (tp.Inherited == 1)
                    qua.TimingPoints.Add(new TimingPointInfo { StartTime = tp.Offset, Bpm = 60000 / tp.MillisecondsPerBeat });

            // Get SliderVelocity Info
            foreach (var tp in osu.TimingPoints)
                if (tp.Inherited == 0)
                    qua.SliderVelocities.Add(new SliderVelocityInfo { StartTime = tp.Offset, Multiplier = (float)Math.Round(0.10 / ((tp.MillisecondsPerBeat / -100) / 10), 2) });

            // Get HitObject Info
            foreach (var hitObject in osu.HitObjects)
            {
                // Get the keyLane the hitObject is in
                var keyLane = 0;

                if (hitObject.Key1)
                    keyLane = 1;
                else if (hitObject.Key2)
                    keyLane = 2;
                else if (hitObject.Key3)
                    keyLane = 3;
                else if (hitObject.Key4)
                    keyLane = 4;
                else if (hitObject.Key5)
                    keyLane = 5;
                else if (hitObject.Key6)
                    keyLane = 6;
                else if (hitObject.Key7)
                    keyLane = 7;

                // Add HitObjects to the list depending on the object type
                switch (hitObject.Type)
                {
                    // Normal HitObjects
                    case 1:
                    case 5:
                        qua.HitObjects.Add(new HitObjectInfo { StartTime = hitObject.StartTime, Lane = keyLane, EndTime = 0 });
                        break;
                    case 128:
                    case 22:
                        qua.HitObjects.Add(new HitObjectInfo { StartTime = hitObject.StartTime, Lane = keyLane, EndTime = hitObject.EndTime });
                        break;
                    default:
                        break;
                }
            }

            // Do a validity check and some final sorting.
            qua.IsValidQua = CheckQuaValidity(qua);
            qua.Sort();

            return qua;
        }
    }

    /// <summary>
    ///     TimingPoints section of the .qua
    /// </summary>
    public class TimingPointInfo
    {
        /// <summary>
        ///     The time in milliseconds for when this timing point begins
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        ///     The BPM during this timing point
        /// </summary>
        public float Bpm { get; set; }
    }

    /// <summary>
    ///     SliderVelocities section of the .qua   
    /// </summary>
    public class SliderVelocityInfo
    {
        /// <summary>
        ///     The time in milliseconds when the new SliderVelocity section begins
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        ///     The velocity multiplier relative to the current timing section's BPM
        /// </summary>
        public float Multiplier { get; set; }
    }

    /// <summary>
    ///     HitObjects section of the .qua
    /// </summary>
    public class HitObjectInfo
    {
        /// <summary>
        ///     The time in milliseconds when the HitObject is supposed to be hit.
        /// </summary>
        public int StartTime { get; set; }

        /// <summary>
        ///     The lane the HitObject falls in
        /// </summary>
        public int Lane { get; set; }

        /// <summary>
        ///     The endtime of the HitObject (if greater than 0, it's considered a hold note.)
        /// </summary>
        public int EndTime { get; set; }
    }
}