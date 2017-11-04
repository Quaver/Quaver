using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Peppy
{
    internal class PeppyBeatmap
    {
        /// <summary>
        ///     The original file name of the .osu
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        ///     Is the peppy beatmap valid?
        /// </summary>
        public bool IsValid { get; set; }

        public string PeppyFileFormat { get; set; }

        // [General]
        public string AudioFilename { get; set; }
        public int AudioLeadIn { get; set; }
        public int PreviewTime { get; set; }
        public int Countdown { get; set; }
        public string SampleSet { get; set; }
        public float StackLeniency { get; set; }
        public int Mode { get; set; }
        public int LetterboxInBreaks { get; set; }
        public int SpecialStyle { get; set; }
        public int WidescreenStoryboard { get; set; }

        // [Editor]
        public string Bookmarks { get; set; }
        public float DistanceSpacing { get; set; }
        public int BeatDivisor { get; set; }
        public int GridSize { get; set; }
        public float TimelineZoom { get; set; }

        // [Metadata]
        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }
        public string Creator { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }
        public string Tags { get; set; }
        public int BeatmapID { get; set; }
        public int BeatmapSetID { get; set; }

        // [Difficulty]
        public float HPDrainRate { get; set; }
        public int KeyCount { get; set; }
        public float OverallDifficulty { get; set; }
        public float ApproachRate { get; set; }
        public float SliderMultiplier { get; set; }
        public float SliderTickRate { get; set; }

        // [Events]
        public string Background { get; set; }

        // [TimingPoints]
        public List<TimingPoint> TimingPoints { get; set; } = new List<TimingPoint>();

        // [HitObjects]
        public List<HitObject> HitObjects { get; set; } = new List<HitObject>();

        /// <summary>
        ///     Ctor - Automatically parses a Peppy beatmap
        /// </summary>
        public PeppyBeatmap(string filePath)
        {
            if (!File.Exists(filePath.Trim()))
            {
                IsValid = false;
                return;
            }

            // Create a new beatmap object and default the validity to true.
            IsValid = true;
            OriginalFileName = filePath;

            // This will hold the section of the beatmap that we are parsing.
            var section = "";

            foreach (var line in File.ReadAllLines(filePath))
            {
                switch (line.Trim())
                {
                    case "[General]":
                        section = "[General]";
                        break;
                    case "[Editor]":
                        section = "[Editor]";
                        break;
                    case "[Metadata]":
                        section = "[Metadata]";
                        break;
                    case "[Difficulty]":
                        section = "[Difficulty]";
                        break;
                    case "[Events]":
                        section = "[Events]";
                        break;
                    case "[TimingPoints]":
                        section = "[TimingPoints]";
                        break;
                    case "[HitObjects]":
                        section = "[HitObjects]";
                        break;
                    case "[Colours]":
                        section = "[Colours]";
                        break;
                    default:
                        break;
                }

                // Parse Peppy file format
                if (line.StartsWith("osu file format"))
                    PeppyFileFormat = line;

                // Parse [General] Section
                if (section.Equals("[General]"))
                {
                    if (line.Contains(":"))
                    {
                        var key = line.Substring(0, line.IndexOf(':'));
                        var value = line.Split(':').Last().Trim();

                        switch (key.Trim())
                        {
                            case "AudioFilename":
                                AudioFilename = value;
                                break;
                            case "AudioLeadIn":
                                AudioLeadIn = Int32.Parse(value);
                                break;
                            case "PreviewTime":
                                PreviewTime = Int32.Parse(value);
                                break;
                            case "Countdown":
                                Countdown = Int32.Parse(value);
                                break;
                            case "SampleSet":
                                SampleSet = value;
                                break;
                            case "StackLeniency":
                                StackLeniency = float.Parse(value);
                                break;
                            case "Mode":
                                Mode = Int32.Parse(value);
                                break;
                            case "LetterboxInBreaks":
                                LetterboxInBreaks = Int32.Parse(value);
                                break;
                            case "SpecialStyle":
                                SpecialStyle = Int32.Parse(value);
                                break;
                            case "WidescreenStoryboard":
                                WidescreenStoryboard = Int32.Parse(value);
                                break;

                        }

                    }

                }

                // Parse [Editor] Data
                if (section.Equals("[Editor]"))
                {
                    if (line.Contains(":"))
                    {
                        var key = line.Substring(0, line.IndexOf(':'));
                        var value = line.Split(':').Last();

                        switch (key.Trim())
                        {
                            case "Bookmarks":
                                Bookmarks = value;
                                break;
                            case "DistanceSpacing":
                                DistanceSpacing = float.Parse(value);
                                break;
                            case "BeatDivisor":
                                BeatDivisor = Int32.Parse(value);
                                break;
                            case "GridSize":
                                GridSize = Int32.Parse(value);
                                break;
                            case "TimelineZoom":
                                TimelineZoom = float.Parse(value);
                                break;
                        }
                    }

                }

                // Parse [Editor] Data
                if (section.Equals("[Metadata]"))
                {
                    if (line.Contains(":"))
                    {
                        var key = line.Substring(0, line.IndexOf(':'));
                        var value = line.Split(':').Last();

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
                            case "Creator":
                                Creator = value;
                                break;
                            case "Version":
                                Version = value;
                                break;
                            case "Source":
                                Source = value;
                                break;
                            case "Tags":
                                Tags = value;
                                break;
                            case "BeatmapID":
                                BeatmapID = Int32.Parse(value);
                                break;
                            case "BeatmapSetID":
                                BeatmapSetID = Int32.Parse(value);
                                break;
                            default:
                                break;
                        }
                    }

                }

                // Parse [Difficulty] Data
                if (section.Equals("[Difficulty]"))
                {
                    if (line.Contains(":"))
                    {
                        var key = line.Substring(0, line.IndexOf(':'));
                        var value = line.Split(':').Last();

                        switch (key.Trim())
                        {
                            case "HPDrainRate":
                                HPDrainRate = float.Parse(value);
                                break;
                            case "CircleSize":
                                KeyCount = Int32.Parse(value);
                                break;
                            case "OverallDifficulty":
                                OverallDifficulty = float.Parse(value);
                                break;
                            case "ApproachRate":
                                ApproachRate = float.Parse(value);
                                break;
                            case "SliderMultiplier":
                                SliderMultiplier = float.Parse(value);
                                break;
                            case "SliderTickRate":
                                SliderTickRate = float.Parse(value);
                                break;
                        }
                    }

                }

                // Parse [Events] Data
                if (section.Equals("[Events]"))
                {
                    // We only care about parsing the background path,
                    // So there's no need to parse the storyboard data.
                    if (line.Contains("png") || line.Contains("jpg") || line.Contains("jpeg"))
                    {
                        string[] values = line.Split(',');
                        Background = values[2];
                    }
                }

                try
                {
                    // Parse [TimingPoints] Data
                    if (section.Equals("[TimingPoints]"))
                    {
                        if (line.Contains(","))
                        {
                            string[] values = line.Split(',');

                            var timingPoint = new TimingPoint();

                            timingPoint.Offset = float.Parse(values[0]);
                            timingPoint.MillisecondsPerBeat = float.Parse(values[1]);
                            timingPoint.Meter = Int32.Parse(values[2]);
                            timingPoint.SampleType = Int32.Parse(values[3]);
                            timingPoint.SampleSet = Int32.Parse(values[4]);
                            timingPoint.Volume = Int32.Parse(values[5]);
                            timingPoint.Inherited = Int32.Parse(values[6]);
                            timingPoint.KiaiMode = Int32.Parse(values[7]);

                            TimingPoints.Add(timingPoint);
                        }
                    }
                }
                catch (Exception e)
                {
                    IsValid = false;
                }

                // Parse [HitObjects] Data
                if (section.Equals("[HitObjects]"))
                {
                    if (line.Contains(","))
                    {
                        string[] values = line.Split(',');

                        // We'll need to parse LNs differently than normal HitObjects,
                        // as they have a different syntax. 128 in the object's type
                        // signifies that it is an LN
                        HitObject hitObject = new HitObject();

                        hitObject.X = Int32.Parse(values[0]);

                        // 4k and 7k have both different hit object parsing.
                        if (KeyCount == 4)
                        {
                            if (hitObject.X >= 0 && hitObject.X <= 127)
                                hitObject.Key1 = true;
                            else if (hitObject.X >= 128 && hitObject.X <= 255)
                                hitObject.Key2 = true;
                            else if (hitObject.X >= 256 && hitObject.X <= 383)
                                hitObject.Key3 = true;

                            else if (hitObject.X >= 384 && hitObject.X <= 511)
                                hitObject.Key4 = true;
                        }
                        // 7k
                        else if (KeyCount == 7)
                        {
                            if (hitObject.X >= 0 && hitObject.X <= 108)
                                hitObject.Key1 = true;
                            else if (hitObject.X >= 109 && hitObject.X <= 181)
                                hitObject.Key2 = true;
                            else if (hitObject.X >= 182 && hitObject.X <= 255)
                                hitObject.Key3 = true;
                            else if (hitObject.X >= 256 && hitObject.X <= 328)
                                hitObject.Key4 = true;
                            else if (hitObject.X >= 329 && hitObject.X <= 401)
                                hitObject.Key5 = true;
                            else if (hitObject.X >= 402 && hitObject.X <= 474)
                                hitObject.Key6 = true;
                            else if (hitObject.X >= 475 && hitObject.X <= 547)
                                hitObject.Key7 = true;
                        }

                        hitObject.Y = Int32.Parse(values[1]);
                        hitObject.StartTime = Int32.Parse(values[2]);
                        hitObject.Type = Int32.Parse(values[3]);
                        hitObject.HitSound = Int32.Parse(values[4]);
                        hitObject.Additions = "0:0:0:0:";

                        // If it's an LN, we'll want to add the object's EndTime as well.
                        if (line.Contains("128"))
                        {
                            var endTime = values[5].Substring(0, values[5].IndexOf(":"));
                            hitObject.EndTime = Int32.Parse(endTime);
                        }

                        HitObjects.Add(hitObject);
                    }
                }
            }
        }

        /// <summary>
        ///     Converts a peppy beatmap to .qua format
        ///     returns if the file was successfully 
        /// </summary>
        /// <returns></returns>
        internal void ConvertToQua()
        {
            var fileExtension = ".qua";

            // We'll need to build a string in the .qua file format. 
            // For file format details, see Misc/file_format.qua
            var fileString = new StringBuilder();

            // # General     
            fileString.AppendLine("# General");
            fileString.AppendLine("AudioFile: " + AudioFilename);
            fileString.AppendLine("AudioLeadIn: " + AudioLeadIn);
            fileString.AppendLine("SongPreviewTime: " + PreviewTime);
            fileString.AppendLine("BackgroundFile: " + Background);
            fileString.AppendLine();

            // # Metadata
            fileString.AppendLine("# Metadata");
            fileString.AppendLine("Title: " + Title);
            fileString.AppendLine("TitleUnicode: " + TitleUnicode);
            fileString.AppendLine("Artist: " + Artist);
            fileString.AppendLine("ArtistUnicode: " + ArtistUnicode);
            fileString.AppendLine("Source: " + Source);
            fileString.AppendLine("Tags: " + Tags);
            fileString.AppendLine("Creator: ");
            fileString.AppendLine("DifficultyName: " + Version);
            fileString.AppendLine("MapID: -1");
            fileString.AppendLine("MapSetID: -1");
            fileString.AppendLine("Description: This beatmap was converted from osu!mania");
            fileString.AppendLine();

            // # Difficulty
            fileString.AppendLine("# Difficulty");
            fileString.AppendLine("KeyCount: " + KeyCount);
            fileString.AppendLine("HPDrain: " + HPDrainRate);
            fileString.AppendLine("AccuracyStrain: " + OverallDifficulty);
            fileString.AppendLine();

            // # Timing (StartTime | BPM)
            fileString.AppendLine("# Timing");
            foreach (var timingPoint in TimingPoints)
            {
                // Find all of the red lined timing points and add their data to the string.
                if (timingPoint.Inherited == 1)
                    fileString.AppendLine(timingPoint.Offset + "|" + 60000 / timingPoint.MillisecondsPerBeat);
            }

            fileString.AppendLine();

            // # SV (StartTime|Multiplier|Volume)
            fileString.AppendLine("# SV");
            foreach (var timingPoint in TimingPoints)
            {
                if (timingPoint.Inherited == 0)
                    fileString.AppendLine(timingPoint.Offset + "|" + Math.Round(0.10 / ((timingPoint.MillisecondsPerBeat / -100) / 10), 2) + "|" + timingPoint.Volume);
            }

            fileString.AppendLine();

            // # HitObject (StartTime|KeyLane|EndTime)
            fileString.AppendLine("# HitObjects");
            foreach (var hitObject in HitObjects)
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

                // Normal HitObjects
                if (hitObject.Type == 1 || hitObject.Type == 5)
                    fileString.AppendLine(hitObject.StartTime + "|" + keyLane + "|" + "0");
                // LNs
                else if (hitObject.Type == 128 || hitObject.Type == 22)
                    fileString.AppendLine(hitObject.StartTime + "|" + keyLane + "|" + hitObject.EndTime);
            }

            try
            {
                // Write the file.
                var file = new StreamWriter(OriginalFileName.Replace(".osu", ".qua"))
                {
                    AutoFlush = true
                };

                file.WriteLine(fileString.ToString());
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    /// <summary>
    ///     Struct for the timing point data.
    /// </summary>
    public struct TimingPoint
    {
        public float Offset { get; set; }
        public float MillisecondsPerBeat { get; set; }
        public int Meter { get; set; }
        public int SampleType { get; set; }
        public int SampleSet { get; set; }
        public int Volume { get; set; }
        public int Inherited { get; set; }
        public int KiaiMode { get; set; }
    }

    /// <summary>
    ///  Struct for all the hit object data.
    /// </summary>
    public struct HitObject
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int StartTime { get; set; }
        public int Type { get; set; }
        public int HitSound { get; set; }
        public int EndTime { get; set; }
        public string Additions { get; set; }
        public bool Key1 { get; set; }
        public bool Key2 { get; set; }
        public bool Key3 { get; set; }
        public bool Key4 { get; set; }
        public bool Key5 { get; set; }
        public bool Key6 { get; set; }
        public bool Key7 { get; set; }
    }
}
