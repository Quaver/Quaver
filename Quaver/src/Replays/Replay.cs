using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.Utility;

namespace Quaver.Replays
{
    internal class Replay
    {
        /// <summary>
        ///     The version of Quaver the replay was created with (MD5 hash of the exe)
        /// </summary>
        internal string QuaverVersion { get; set; }

        /// <summary>
        ///     The MD5 of the beatmap played 
        /// </summary>
        internal string BeatmapMd5 { get; set; }

        /// <summary>
        ///     The MD5 hash of the actual replay
        /// </summary>
        internal string ReplayMd5 { get; set; }

        /// <summary>
        ///     The name of the player that made the replay
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        ///     The date and time the replay was made
        /// </summary>
        internal DateTime Date { get; set; }

        /// <summary>
        ///     The bitwise combination of mods that were used during the replay
        /// </summary>
        internal ModIdentifier Mods { get; set; }

        /// <summary>
        ///     The scroll speed the player used during this play.
        /// </summary>
        public int ScrollSpeed { get; set; }

        /// <summary>
        ///     The score of the replay
        /// </summary>
        internal int Score { get; set; }

        /// <summary>
        ///     The accuracy achieved on the replay
        /// </summary>
        internal float Accuracy { get; set; }
        
        /// <summary>
        ///     The max combo achieved on the replay
        /// </summary>
        internal int MaxCombo { get; set; }

        /// <summary>
        ///     The number of marv presses
        /// </summary>
        public int MarvPressCount { get; set; }

        /// <summary>
        ///     The number of marv releases
        /// </summary>
        public int MarvReleaseCount { get; set; }

        /// <summary>
        ///     The number of Perf presses
        /// </summary>
        public int PerfPressCount { get; set; }

        /// <summary>
        ///     The number of perf releases
        /// </summary>
        public int PerfReleaseCount { get; set; }

        /// <summary>
        ///     The number of great presses
        /// </summary>
        public int GreatPressCount { get; set; }

        /// <summary>
        ///     The number of great releases
        /// </summary>
        public int GreatReleaseCount { get; set; }

        /// <summary>
        ///     The number of good presses
        /// </summary>
        public int GoodPressCount { get; set; }

        /// <summary>
        ///     The number of good releases
        /// </summary>
        public int GoodReleaseCount { get; set; }

        /// <summary>
        ///     The number of okay presses
        /// </summary>
        public int OkayPressCount { get; set; }

        /// <summary>
        ///     The number of okay releases
        /// </summary>
        public int OkayReleaseCount { get; set; }

        /// <summary>
        ///     The number of misses for the score.
        /// </summary>
        public int Misses { get; set; }

        /// <summary>
        ///     The list of replay frame data contained in the replay
        /// </summary>
        public List<ReplayFrame> ReplayFrames { get; set; }

        /// <summary>
        ///     Stores the path of the test replay file
        /// </summary>
        internal static string DebugFilePath { get; } = Configuration.DataDirectory + "/" + "last_replay.txt";

        /// <summary>
        ///     Writes the replay to a binary file (.qur)
        /// </summary>
        internal void Write(string fileName)
        {
            // Create the full path
            var path = Configuration.ReplayDirectory + "/" + Util.FileNameSafeString(fileName) + ".qur";
            Logger.Log(path, Color.AliceBlue);

            using (var replayDataStream = new MemoryStream(Encoding.ASCII.GetBytes(ReplayFramesToString())))
            using (var fs = new FileStream(path, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(QuaverVersion);
                bw.Write(BeatmapMd5);
                bw.Write(ReplayMd5);
                bw.Write(Name);
                bw.Write(Date.ToString(CultureInfo.InvariantCulture));
                bw.Write((int)Mods);
                bw.Write(ScrollSpeed);
                bw.Write(Score);
                bw.Write(Accuracy);
                bw.Write(MaxCombo);
                bw.Write(MarvPressCount);
                bw.Write(MarvReleaseCount);
                bw.Write(PerfPressCount);
                bw.Write(PerfReleaseCount);
                bw.Write(GreatPressCount);
                bw.Write(GreatReleaseCount);
                bw.Write(GoodPressCount);
                bw.Write(GoodReleaseCount);
                bw.Write(OkayPressCount);
                bw.Write(OkayReleaseCount);
                bw.Write(Misses);
                SevenZip.Helper.Compress(replayDataStream, fs);
            }
        }

        /// <summary>
        ///     Writes the replay to a log file if in debug mode
        /// </summary>
        internal void WriteToLogFile(string path = "")
        {
            if (!Configuration.Debug)
                return;

            // Create file and close it.
            if (path == "")
                path = DebugFilePath;

            var file = File.Create(path);
            file.Close();

            var sw = new StreamWriter(path, true)
            {
                AutoFlush = true
            };

            foreach (var frame in ReplayFrames)
                sw.WriteLine($"{frame.SongTime}|{frame.KeyPressState.ToString()}");

            sw.Close();
        }

        /// <summary>
        ///     Converts all replay frames to a string
        /// </summary>
        private string ReplayFramesToString()
        {
            // The format for the replay frames are the following:
            //      SongTime|KeysPressed,
            var frameStr = "";

            ReplayFrames.ForEach(x => frameStr += $"{x.SongTime}|{(int)x.KeyPressState},");

            return frameStr;
        }
    }
}
