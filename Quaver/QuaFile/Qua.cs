using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quaver.QuaFile
{
    internal class Qua
    {
        /// <summary>
        ///     The difficulty of accuracy for the map.
        /// </summary>
        internal float AccuracyStrain { get; set; }

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
        ///     The amount of hitpoints drain on the map.
        /// </summary>
        internal float HpDrain { get; set; }

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

        // Constructor
        internal Qua(string filePath, bool gameplay = false)
        {
            Parse(filePath, gameplay);
        }

        /// <summary>
        /// Parses the specified .qua file in the constructor.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="gameplay"></param>
        /// <returns></returns>
        private void Parse(string filePath, bool gameplay)
        {
            // If the file doesn't exist, or it doesn't have a .qua extension, 
            // consider that an invalid Qua
            if (!File.Exists(filePath) || !filePath.ToLower().EndsWith(".qua"))
                IsValidQua = false;

            // This will hold the current file section that we are parsing
            var fileSection = "";

            // Loop through all of the lines in the .qua file and begin parsing
            using (var sr = new StreamReader(filePath))
            {
                while (sr.Peek() >= 0)
                {
                    // Read the current file line and get the file section associated with it.
                    var line = sr.ReadLine();
                    fileSection = GetFileSection(line, fileSection);

                    // If we're parsing specifically for gameplay.
                    if (gameplay)
                        ParseQuaForGameplay(fileSection, line);
                    else
                        ParseEntireQua(fileSection, line);
                }
            }
        }

        /// <summary>
        /// Runs a check and modifies the current section of the file we are parsing.
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        /// <param name="currentSection">The current section of the file.</param>
        /// <returns></returns>
        private static string GetFileSection(string line, string currentSection)
        {
            switch (line.Trim())
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
        /// This will parse the entire .qua file containing all the information, rather than to it's counterpart
        /// ParseQuaForGampeplay, which only parses the required data used for gameplay
        /// </summary>
        /// <param name="fileSection">The current file section</param>
        /// <param name="line">The current line of the file.</param>
        private void ParseEntireQua(string fileSection, string line)
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

        /// <summary>
        /// This will parse only the required information in the .qua for gameplay purposes
        /// </summary>
        /// <param name="fileSection">The current file section</param>
        /// <param name="line">The current line of the file.</param>
        private void ParseQuaForGameplay(string fileSection, string line)
        {
            switch (fileSection)
            {
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

        /// <summary>
        /// Parses the #General section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseGeneral(string line)
        {
            if (line.Contains(":"))
            {
                var key = line.Substring(0, line.IndexOf(':'));
                var value = line.Trim().Split(':').Last().Trim();

                switch (key.Trim())
                {
                    case "AudioFile":
                        AudioFile = value;
                        break;
                    case "AudioLeadIn":
                        AudioLeadIn = Int32.Parse(value);
                        break;
                    case "SongPreviewTime":
                        SongPreviewTime = Int32.Parse(value);
                        break;
                    case "BackgroundFile":
                        BackgroundFile = value;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the #Metadata section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseMetadata(string line)
        {
            if (line.Contains(":"))
            {
                var key = line.Substring(0, line.IndexOf(':'));
                var value = line.Trim().Split(':').Last().Trim();

                switch (key.Trim())
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
                        MapId = Int32.Parse(value);
                        break;
                    case "MapSetID":
                        MapSetId = Int32.Parse(value);
                        break;
                    case "Description":
                        Description = value;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the #Difficulty section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseDifficulty(string line)
        {
            if (line.Contains(":"))
            {
                var key = line.Substring(0, line.IndexOf(':'));
                var value = line.Trim().Split(':').Last().Trim();

                switch (key.Trim())
                {
                    case "HPDrain":
                        HpDrain = float.Parse(value);
                        break;
                    case "AccuracyStrain":
                        AccuracyStrain = float.Parse(value);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the #Timing section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseTiming(string line)
        {
            if (line.Contains("|") && !line.Contains("#"))
            {
                string[] values = line.Split('|');

                if (values.Length != 2)
                    IsValidQua = false;

                var timing = new TimingPoint()
                {
                    StartTime = float.Parse(values[0]),
                    Bpm = float.Parse(values[1])
                };

                TimingPoints.Add(timing);
            }
        }

        /// <summary>
        /// Parses the #SV section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseSliderVelocity(string line)
        {
            if (line.Contains("|") && !line.Contains("#"))
            {
                string[] values = line.Split('|');

                // There should only be 3 values in an SV, if not, it's an invalid map.
                if (values.Length != 3)
                    IsValidQua = false;

                var sv = new SliderVelocity()
                {
                    StartTime = float.Parse(values[0]),
                    Multiplier = float.Parse(values[1]),
                    Volume = Int32.Parse(values[2])
                };

                SliderVelocities.Add(sv);
            }
        }

        /// <summary>
        /// Parses the #HitObject section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        private void ParseHitObject(string line)
        {
            if (line.Contains("|") && !line.Contains("HitObjects"))
            {
                string[] values = line.Split('|');

                if (values.Length != 3)
                    IsValidQua = false;

                var ho = new HitObject()
                {
                    StartTime = Int32.Parse(values[0]),
                    KeyLane = Int32.Parse(values[1])
                };

                // If the key lane isn't in 1-4, then we'll consider the map to be invalid.
                if (ho.KeyLane < 1 || ho.KeyLane > 4)
                    IsValidQua = false;

                ho.EndTime = Int32.Parse(values[2]);

                HitObjects.Add(ho);
            }
        }

        /// <summary>
        /// Responsible for checking the validity of a QuaFile.
        /// </summary>
        private void CheckQuaValidity()
        {
            // If there aren't any HitObjects
            if (HitObjects.Count == 0)
                IsValidQua = false;

            // If there aren't any TimingPoints
            if (TimingPoints.Count == 0)
                IsValidQua = false;
        }
    }
}