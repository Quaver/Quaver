﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Logging;

namespace Quaver.QuaFile
{
    internal class Qua
    {
        // Constructor
        internal Qua(string filePath)
        {
            if (!File.Exists(filePath))
            {
                IsValidQua = false;
                return;
            }

            Parse(filePath);
        }

        /// <summary>
        ///     The difficulty of accuracy for the map.
        /// </summary>
        internal float Judge { get; set; }

        /// <summary>
        ///     The artist of the song.
        /// </summary>
        internal string Artist { get; set; }

        /// <summary>
        ///     The unicode artist of the song.
        /// </summary>
        internal string ArtistUnicode { get; set; }

        /// <summary>
        ///     The name of the audio file (.ogg)
        /// </summary>
        internal string AudioFile { get; set; }

        /// <summary>
        ///     The amount of time before the audio starts.
        /// </summary>
        internal int AudioLeadIn { get; set; }

        /// <summary>
        ///     The file name of the song background.
        /// </summary>
        internal string BackgroundFile { get; set; }

        /// <summary>
        ///     The creator of the beatmap
        /// </summary>
        internal string Creator { get; set; }

        /// <summary>
        ///     A user defined description of the map.
        /// </summary>
        internal string Description { get; set; }

        /// <summary>
        ///     The name of the difficulty for the beatmap
        /// </summary>
        internal string DifficultyName { get; set; }

        /// <summary>
        ///     The physical hit objects in the map.
        /// </summary>
        internal List<HitObject> HitObjects { get; set; } = new List<HitObject>();

        /// <summary>
        ///     The key count for the map (Quaver only supports 4k and 7k)
        /// </summary>
        internal int KeyCount { get; set; }

        /// <summary>
        ///     Keeps track of whether or not the data in the QuaFile object is valid.
        /// </summary>
        internal bool IsValidQua { get; set; } = true;

        /// <summary>
        ///     The map's unique identifier if it was uploaded.
        /// </summary>
        internal int MapId { get; set; }

        /// <summary>
        ///     The mapset's unique identifier if it was uploaded.
        /// </summary>
        internal int MapSetId { get; set; }

        /// <summary>
        ///     The points in the map where the SV changes.
        /// </summary>
        internal List<SliderVelocity> SliderVelocities { get; set; } = new List<SliderVelocity>();

        /// <summary>
        ///     The offset of the song which will be used to play a preview during song select
        /// </summary>
        internal int SongPreviewTime { get; set; }

        /// <summary>
        ///     The source of the song (Album, Mixtape, etc.)
        /// </summary>
        internal string Source { get; set; }

        /// <summary>
        ///     Specific tags for the song (Used when searching)
        /// </summary>
        internal string Tags { get; set; }

        /// <summary>
        ///     All of the map's timing sections.
        /// </summary>
        internal List<TimingPoint> TimingPoints { get; set; } = new List<TimingPoint>();

        /// <summary>
        ///     The title of the song.
        /// </summary>
        internal string Title { get; set; }

        /// <summary>
        ///     The unicode title of the song.
        /// </summary>
        internal string TitleUnicode { get; set; }

        /// <summary>
        /// Asynchronously parses a .qua file and creates a Qua object
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static async Task<Qua> Create(string filePath)
        {
            return await Task.Run(() => new Qua(filePath));
        }

        /// <summary>
        ///     Parses the specified .qua file in the constructor.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="gameplay"></param>
        /// <returns></returns>
        private void Parse(string filePath)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // If the file doesn't exist, or it doesn't have a .qua extension, 
            // consider that an invalid Qua
            if (!File.Exists(filePath) || !filePath.ToLower().EndsWith(".qua"))
            {
                IsValidQua = false;
                return;
            }

            // This will hold the current file section that we are parsing
            var fileSection = "";

            // Loop through all of the lines in the .qua file and begin parsing
            using (var sr = new StreamReader(filePath))
            {
                while (sr.Peek() >= 0)
                {
                    // Read the current file line and get the file section associated with it.
                    var line = sr.ReadLine().Trim();

                    if (line == "")
                        continue;

                    fileSection = GetFileSection(line, fileSection);

                    ParseEntireQua(fileSection, line);
                }
            }

            // Run a final validity check on the Qua object.
            CheckQuaValidity();
        }

        /// <summary>
        ///     Runs a check and modifies the current section of the file we are parsing.
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        /// <param name="currentSection">The current section of the file.</param>
        /// <returns></returns>
        private static string GetFileSection(string line, string currentSection)
        {
            switch (line)
            {
                case "# General":
                    return "General";
                case "# Metadata":
                    return "Metadata";
                case "# Difficulty":
                    return "Difficulty";
                case "# Timing":
                    return "Timing";
                case "# SV":
                    return "SV";
                case "# HitObjects":
                    return "HitObjects";
                default:
                    return currentSection;
            }
        }

        /// <summary>
        ///     This will parse the entire .qua file containing all the information, rather than to it's counterpart
        ///     ParseQuaForGampeplay, which only parses the required data used for gameplay
        /// </summary>
        /// <param name="fileSection">The current file section</param>
        /// <param name="line">The current line of the file.</param>
        private void ParseEntireQua(string fileSection, string line)
        {
            try
            {
                switch (fileSection)
                {
                    case "General":
                        ParseGeneral(line);
                        break;
                    case "Metadata":
                        ParseMetadata(line);
                        break;
                    case "Difficulty":
                        ParseDifficulty(line);
                        break;
                    case "Timing":
                        ParseTiming(line);
                        break;
                    case "SV":
                        ParseSliderVelocity(line);
                        break;
                    case "HitObjects":
                        ParseHitObject(line);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red);
                throw;
            }
        }

        /// <summary>
        ///     Parses the #General section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseGeneral(string line)
        {
            if (!line.Contains(":"))
                return;

            var key = line.Substring(0, line.IndexOf(':')).Trim();
            var value = line.Split(':').Last().Trim();

            switch (key)
            {
                case "AudioFile":
                    AudioFile = value;
                    break;
                case "AudioLeadIn":
                    AudioLeadIn = int.Parse(value);
                    break;
                case "SongPreviewTime":
                    SongPreviewTime = int.Parse(value);
                    break;
                case "BackgroundFile":
                    BackgroundFile = value.Replace("\"", "");
                    break;
                default:
                    break;
            }        
        }

        /// <summary>
        ///     Parses the #Metadata section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseMetadata(string line)
        {
            if (!line.Contains(":"))
                return;

            var key = line.Substring(0, line.IndexOf(':')).Trim();
            var value = line.Split(':').Last().Trim();

            switch (key)
            {
                case "Title":
                    Title = value;
                    break;
                case "TitleUnicode":
                    TitleUnicode = value;
                    break;
                case "Artist":
                    Artist = value;
                    break;
                case "ArtistUnicode":
                    ArtistUnicode = value;
                    break;
                case "Source":
                    Source = value;
                    break;
                case "Tags":
                    Tags = value;
                    break;
                case "Creator":
                    Creator = value;
                    break;
                case "DifficultyName":
                    DifficultyName = value;
                    break;
                case "MapID":
                    MapId = int.Parse(value);
                    break;
                case "MapSetID":
                    MapSetId = int.Parse(value);
                    break;
                case "Description":
                    Description = value;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Parses the #Difficulty section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseDifficulty(string line)
        {
            if (!line.Contains(":"))
                return;

            var key = line.Substring(0, line.IndexOf(':')).Trim();
            var value = line.Split(':').Last().Trim();

            switch (key)
            {
                case "AccuracyStrain":
                case "Judge":
                    Judge = float.Parse(value);
                    break;
                case "KeyCount":
                    KeyCount = int.Parse(value);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Parses the #Timing section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseTiming(string line)
        {
            try
            {
                if (!line.Contains("|") || line.Contains("#"))
                    return;

                var values = line.Trim().Split('|');

                if (values.Length != 2)
                    IsValidQua = false;

                var timing = new TimingPoint
                {
                    StartTime = float.Parse(values[0]),
                    Bpm = float.Parse(values[1])
                };

                TimingPoints.Add(timing);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red);
                IsValidQua = false;
            }
        }

        /// <summary>
        ///     Parses the #SV section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseSliderVelocity(string line)
        {
            try
            {
                if (!line.Contains("|") || line.Contains("#"))
                    return;

                var values = line.Trim().Split('|');

                // There should only be 3 values in an SV, if not, it's an invalid map.
                if (values.Length != 3)
                    IsValidQua = false;

                var sv = new SliderVelocity
                {
                    StartTime = float.Parse(values[0]),
                    Multiplier = float.Parse(values[1]),
                    Volume = int.Parse(values[2])
                };

                SliderVelocities.Add(sv);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red);
                IsValidQua = false;
            }
        }

        /// <summary>
        ///     Parses the #HitObject section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseHitObject(string line)
        {
            try
            {
                if (!line.Contains("|") || line.Contains("HitObjects"))
                    return;

                var values = line.Trim().Split('|');

                if (values.Length != 3)
                    IsValidQua = false;

                var ho = new HitObject
                {
                    StartTime = int.Parse(values[0]),
                    KeyLane = int.Parse(values[1])
                };

                // If the key lane isn't in 1-4, then we'll consider the map to be invalid.
                if (ho.KeyLane < 1 || ho.KeyLane > 7)
                    IsValidQua = false;

                ho.EndTime = int.Parse(values[2]);

                HitObjects.Add(ho);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red);
                IsValidQua = false;
            }
        }

        /// <summary>
        ///     Responsible for checking the validity of a QuaFile.
        /// </summary>
        private void CheckQuaValidity()
        {
            // If there aren't any HitObjects
            if (HitObjects.Count == 0)
                IsValidQua = false;

            // If there aren't any TimingPoints
            if (TimingPoints.Count == 0)
                IsValidQua = false;

            // Check for bad key counts. We only support 4 and 7k
            if (KeyCount != 4 && KeyCount != 7)
                IsValidQua = false;
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
            for (var i = GameBase.SelectedBeatmap.Qua.HitObjects.Count - 1; i > 0; i--)
            {
                var ho = GameBase.SelectedBeatmap.Qua.HitObjects[i];
                if (ho.EndTime > LastNoteEnd)
                    LastNoteEnd = ho.EndTime;
                else if (ho.StartTime > LastNoteEnd)
                    LastNoteEnd = ho.StartTime;
            }

            return LastNoteEnd;
        }
    }
}