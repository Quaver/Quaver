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
        // This is responsible for parsing a .qua file. 
        // It only parses the Difficulty, Timing points, SVs, and HitObjects.
        // We don't bother parsing the entire file in this circumstance.
        public static QuaFile Parse(string filePath, bool gameplay = false)
        {
            // Run a check if the file exists, 
            if (!File.Exists(filePath) || !filePath.ToLower().EndsWith(".qua"))
            {
                QuaFile invalidQua = new QuaFile();
                invalidQua.IsValidQua = false;
                return invalidQua;
            }

            QuaFile playableMap = InitializeValidQuaFile();

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
                    ParseQuaForGeneral(fileSection, line, playableMap);
                // In this case, we're parsing the file for gameplay, so we only want to extract
                // certain values.
                else
                    ParseQuaForGameplay(fileSection, line, playableMap);

                // If the map at any point in time becomes invalid, we need to return it
                if (!playableMap.IsValidQua)
                    return playableMap;
            }

            // Do some validity checks here on the map, to see if it was correctly parsed
            // before returning
            CheckQuaValidity(playableMap);

            Debug.Log("[QUA PARSER] Successfully parsed file with validity: " + playableMap.IsValidQua + 
                        "Gameplay?: " + gameplay +" | Path: " + filePath);

            // Finally return the map.
            return playableMap;
        }

        // Responsible for handling the initialization of a valid QuaFile
        private static QuaFile InitializeValidQuaFile()
        {
            // Initialize the new playuable Qua File
            QuaFile validQua = new QuaFile();
            validQua.TimingPoints = new List<TimingPoint>();
            validQua.SliderVelocities = new List<SliderVelocity>();
            validQua.HitObjects = new List<HitObject>();
            validQua.IsValidQua = true;

            return validQua;
        }

        // Runs a check on the current line of the file, and checks which section of the file we are on.
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
        
        // This will parse the entire .qua file containing all the information, rather than to it's counterpart
        // ParseQuaForGampeplay, which only parses the required data used for gameplay
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

        // This will parse only the required information in the .qua for gameplay purposes
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

        // Responsible for parsing ONLY the general section of the map.
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

        // Responbile for parsing ONLY the Metadata section of the file.
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

        // Responsible for parsing on the difficulty section of the map.
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

        // Responsible for parsing the TimingPoints of the map.
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

        // Responsible for parsing the SV section of the .qua file.
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

        // Responsible for parsing a HitObject line of the .qua file
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

        // Responsible for checking the validity of a QuaFile.
        // This should be called after fully parsing the file.
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