using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;

namespace Quaver.Qua
{
    public static class QuaParser
    {
        /// <summary>
        /// Parses a .Qua file and returns an instance of the QuaFile class
        /// </summary>
        /// <param name="filePath">The path of the .qua file you are wanting to parse.</param>
        /// <param name="gameplay">If true, only HitObjects, TimingPoints, SVs, and Difficulty will be parsed</param>
        /// <returns>QuaFile Object</returns>
        public static QuaFile Parse(string filePath, bool gameplay = false)
        {
            // Run a check if the file exists, 
            if (!File.Exists(filePath) || !filePath.ToLower().EndsWith(".qua"))
            {
                QuaFile invalidQua = new QuaFile();
                invalidQua.IsValidQua = false;
                return invalidQua;
            }

            QuaFile playableMap = new QuaFile();

            // This will hold the current file section that we are looking to parse.
            string fileSection = "";

            // Loop through all of the lines in the file, and begin parsing.
            foreach (string line in File.ReadAllLines(filePath))
            {
                // Get the current section of the file.
                fileSection = GetFileSection(line, fileSection);

                // For each individual line in the file, you'll want to run a given parsing method
                // based on which section of the file we are on.
                // In this case, we're parsing the entire file (Not Gameplay)
                if (!gameplay)
                    ParseQuaForGeneral(fileSection, line,  playableMap);
                // In this case, we're parsing the file for gameplay, so we only want to extract
                // certain values.
                else
                    ParseQuaForGameplay(fileSection, line,  playableMap);

                // If the map at any point in time becomes invalid, we need to return it
                if (!playableMap.IsValidQua)
                    return playableMap;
            }

            // Do some validity checks here on the map, to see if it was correctly parsed
            // before returning
            CheckQuaValidity(playableMap);

            // Finally return the map.
            return playableMap;
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
            }

            // If the line we are currently on isn't a section header, just return the current section.
            return currentSection;
        }

        /// <summary>
        /// This will parse the entire .qua file containing all the information, rather than to it's counterpart
        /// ParseQuaForGampeplay, which only parses the required data used for gameplay
        /// </summary>
        /// <param name="fileSection">The current file section</param>
        /// <param name="line">The current line of the file.</param>
        /// <param name="map">The instance of the QuaFile we've created</param>
        private static void ParseQuaForGeneral(string fileSection, string line, QuaFile map)
        {
            switch (fileSection)
            {
                case "General":
                    ParseGeneral(line, map);
                    break;
                case "Metadata":
                    ParseMetadata(line, map);
                    break;
                case "Difficulty":
                    ParseDifficulty(line, map);
                    break;
                case "Timing":
                    ParseTiming(line, map);
                    break;
                case "SV":
                    ParseSV(line, map);
                    break;
                case "HitObjects":
                    ParseHitObject(line, map);
                    break;
            }
        }

        /// <summary>
        /// This will parse only the required information in the .qua for gameplay purposes
        /// </summary>
        /// <param name="fileSection">The current file section</param>
        /// <param name="line">The current line of the file.</param>
        /// <param name="map">The instance of the QuaFile we've created</param>
        private static void ParseQuaForGameplay(string fileSection, string line, QuaFile map)
        {
            switch (fileSection)
            {
                case "Difficulty":
                    ParseDifficulty(line, map);
                    break;
                case "Timing":
                    ParseTiming(line, map);
                    break;
                case "SV":
                    ParseSV(line, map);
                    break;
                case "HitObjects":
                    ParseHitObject(line, map);
                    break;
            }
        }

        /// <summary>
        /// Parses the #General section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        /// <param name="qua">The QuaFile instance we're returning.</param>
        private static void ParseGeneral(string line, QuaFile qua)
        {
            if (line.Contains(":"))
            {
                string key = line.Substring(0, line.IndexOf(':'));
                string value = line.Trim().Split(':').Last().Trim();

                switch (key.Trim())
                {
                    case "AudioFile":
                        qua.AudioFile = value;
                        break;
                    case "AudioLeadIn":
                        qua.AudioLeadIn = Int32.Parse(value);
                        break;
                    case "SongPreviewTime":
                        qua.SongPreviewTime = Int32.Parse(value);
                        break;
                    case "BackgroundFile":
                        qua.BackgroundFile = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the #Metadata section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        /// <param name="qua">The QuaFile instance we're returning.</param>
        private static void ParseMetadata(string line, QuaFile qua)
        {
            if (line.Contains(":"))
            {
                string key = line.Substring(0, line.IndexOf(':'));
                string value = line.Trim().Split(':').Last().Trim();

                switch (key.Trim())
                {
                    case "Title":
                        qua.Title = value;
                        break;
                    case "TitleUnicode":
                        qua.TitleUnicode = value;
                        break;
                    case "Artist":
                        qua.Artist = value;
                        break;
                    case "ArtistUnicode":
                        qua.ArtistUnicode = value;
                        break;
                    case "Source":
                        qua.Source = value;
                        break;
                    case "Tags":
                        qua.Tags = value;
                        break;
                    case "Creator":
                        qua.Creator = value;
                        break;
                    case "DifficultyName":
                        qua.DifficultyName = value;
                        break;
                    case "MapID":
                        qua.MapID = Int32.Parse(value);
                        break;
                    case "MapSetID":
                        qua.MapSetID = Int32.Parse(value);
                        break;
                    case "Description":
                        qua.Description = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the #Difficulty section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        /// <param name="qua">The QuaFile instance we're returning.</param>
        private static void ParseDifficulty(string line, QuaFile qua)
        {
            if (line.Contains(":"))
            {
                string key = line.Substring(0, line.IndexOf(':'));
                string value = line.Trim().Split(':').Last().Trim();

                switch (key.Trim())
                {
                    case "HPDrain":
                        qua.HPDrain = float.Parse(value);
                        break;
                    case "AccuracyStrain":
                        qua.AccuracyStrain = float.Parse(value);
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the #Timing section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        /// <param name="qua">The QuaFile instance we're returning.</param>
        private static void ParseTiming(string line, QuaFile qua)
        {
            if (line.Contains("|") && !line.Contains("#"))
            {
                string[] values = line.Split('|');

                if (values.Length != 2)
                {
                    qua.IsValidQua = false;
                }

                TimingPoint timing = new TimingPoint();

                timing.StartTime = float.Parse(values[0]);
                timing.BPM = float.Parse(values[1]);

                qua.TimingPoints.Add(timing);
            }
        }

        /// <summary>
        /// Parses the #SV section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        /// <param name="qua">The QuaFile instance we're returning.</param>
        private static void ParseSV(string line, QuaFile qua)
        {
            if (line.Contains("|") && !line.Contains("#"))
            {
                string[] values = line.Split('|');

                // There should only be 3 values in an SV, if not, it's an invalid map.
                if (values.Length != 3)
                {
                    qua.IsValidQua = false;
                }

                SliderVelocity sv = new SliderVelocity();

                sv.StartTime = float.Parse(values[0]);
                sv.Multiplier = float.Parse(values[1]);
                sv.Volume = Int32.Parse(values[2]);

                qua.SliderVelocities.Add(sv);
            }
        }

        /// <summary>
        /// Parses the #HitObject section of the file
        /// </summary>
        /// <param name="line">The current line of the file.</param>
        /// <param name="qua">The QuaFile instance we're returning.</param>
        private static void ParseHitObject(string line, QuaFile qua)
        {
            if (line.Contains("|") && !line.Contains("HitObjects"))
            {
                string[] values = line.Split('|');

                if (values.Length != 3)
                {
                    qua.IsValidQua = false;
                }

                HitObject ho = new HitObject(); // lol, ho. 

                ho.StartTime = Int32.Parse(values[0]);
                ho.KeyLane = Int32.Parse(values[1]);

                // If the key lane isn't in 1-4, then we'll consider the map to be invalid.
                if (ho.KeyLane < 1 || ho.KeyLane > 4)
                {
                    qua.IsValidQua = false;
                }

                ho.EndTime = Int32.Parse(values[2]);

                qua.HitObjects.Add(ho);
            }
        }

        /// <summary>
        /// Responsible for checking the validity of a QuaFile.
        /// </summary>
        /// <param name="qua">The instance of the QuaFile object we're validity checking</param>
        private static void CheckQuaValidity(QuaFile qua)
        {
            // If there aren't any HitObjects
            if (qua.HitObjects.Count == 0)
            {
                qua.IsValidQua = false;
            }

            // If there aren't any TimingPoints
            if (qua.TimingPoints.Count == 0)
            {
                qua.IsValidQua = false;
            }
        }
    }
}