// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Quaver.Osu.Beatmap
{
    public class OsuBeatmapParser : MonoBehaviour
    {
        public static OsuBeatmap Parse(string filePath)
        {
            byte osuOffset = 170;

            if (!File.Exists(filePath.Trim()))
            {
                OsuBeatmap tempBeatmap = new OsuBeatmap();
                tempBeatmap.IsValid = false;

                return tempBeatmap;
            }

            // Create a new beatmap object and default the validity to true.
            OsuBeatmap beatmap = new OsuBeatmap();
            beatmap.IsValid = true;

            // Create a new list of timing points which we wll add to below.
            beatmap.TimingPoints = new List<TimingPoint>();

            // Create a new list of Hit Objects which we'll add to below as well.
            beatmap.HitObjects = new List<HitObject>();

            // This will hold the section of the beatmap that we are parsing.
            string section = "";

            foreach (string line in File.ReadAllLines(filePath))
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
                }

                // Parse osu! file format
                if (line.StartsWith("osu file format"))
                {
                    beatmap.OsuFileFormat = line;
                }

                // Parse [General] Section
                if (section.Equals("[General]"))
                {
                    if (line.Contains(":"))
                    {
                        string key = line.Substring(0, line.IndexOf(':'));
                        string value = line.Split(':').Last().Trim();

                        switch (key.Trim())
                        {
                            case "AudioFilename":
                                if (value.Contains(".mp3"))
                                {
                                    value = value.Replace(".mp3", ".ogg");
                                }
                                beatmap.AudioFilename = value;
                                break;
                            case "AudioLeadIn":
                                beatmap.AudioLeadIn = Int32.Parse(value);
                                break;
                            case "PreviewTime":
                                beatmap.PreviewTime = Int32.Parse(value);
                                break;
                            case "Countdown":
                                beatmap.Countdown = Int32.Parse(value);
                                break;
                            case "SampleSet":
                                beatmap.SampleSet = value;
                                break;
                            case "StackLeniency":
                                beatmap.StackLeniency = float.Parse(value);
                                break;
                            case "Mode":
                                beatmap.Mode = Int32.Parse(value);
                                break;
                            case "LetterboxInBreaks":
                                beatmap.LetterboxInBreaks = Int32.Parse(value);
                                break;
                            case "SpecialStyle":
                                beatmap.SpecialStyle = Int32.Parse(value);
                                break;
                            case "WidescreenStoryboard":
                                beatmap.WidescreenStoryboard = Int32.Parse(value);
                                break;
                        }
                    }
                }

                // Parse [Editor] Data
                if (section.Equals("[Editor]"))
                {
                    if (line.Contains(":"))
                    {
                        string key = line.Substring(0, line.IndexOf(':'));
                        string value = line.Split(':').Last();

                        switch (key.Trim())
                        {
                            case "Bookmarks":
                                beatmap.Bookmarks = value;
                                break;
                            case "DistanceSpacing":
                                beatmap.DistanceSpacing = float.Parse(value);
                                break;
                            case "BeatDivisor":
                                beatmap.BeatDivisor = Int32.Parse(value);
                                break;
                            case "GridSize":
                                beatmap.GridSize = Int32.Parse(value);
                                break;
                            case "TimelineZoom":
                                beatmap.TimelineZoom = float.Parse(value);
                                break;
                        }
                    }
                }

                // Parse [Editor] Data
                if (section.Equals("[Metadata]"))
                {
                    if (line.Contains(":"))
                    {
                        string key = line.Substring(0, line.IndexOf(':'));
                        string value = line.Split(':').Last();

                        switch (key.Trim())
                        {
                            case "Title":
                                beatmap.Title = value;
                                break;
                            case "TitleUnicode":
                                beatmap.TitleUnicode = value;
                                break;
                            case "Artist":
                                beatmap.Artist = value;
                                break;
                            case "ArtistUnicode":
                                beatmap.ArtistUnicode = value;
                                break;
                            case "Creator":
                                beatmap.Creator = value;
                                break;
                            case "Version":
                                beatmap.Version = value;
                                break;
                            case "Source":
                                beatmap.Source = value;
                                break;
                            case "Tags":
                                beatmap.Tags = value;
                                break;
                            case "BeatmapID":
                                beatmap.BeatmapID = Int32.Parse(value);
                                break;
                            case "BeatmapSetID":
                                beatmap.BeatmapSetID = Int32.Parse(value);
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
                        string key = line.Substring(0, line.IndexOf(':'));
                        string value = line.Split(':').Last();

                        switch (key.Trim())
                        {
                            case "HPDrainRate":
                                beatmap.HPDrainRate = float.Parse(value);
                                break;
                            case "CircleSize":
                                beatmap.KeyCount = Int32.Parse(value);
                                break;
                            case "OverallDifficulty":
                                beatmap.OverallDifficulty = float.Parse(value);
                                break;
                            case "ApproachRate":
                                beatmap.ApproachRate = float.Parse(value);
                                break;
                            case "SliderMultiplier":
                                beatmap.SliderMultiplier = float.Parse(value);
                                break;
                            case "SliderTickRate":
                                beatmap.SliderTickRate = float.Parse(value);
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
                        beatmap.Background = values[2];
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

                            TimingPoint timingPoint = new TimingPoint();

                            timingPoint.Offset = float.Parse(values[0]) + osuOffset;
                            timingPoint.MillisecondsPerBeat = float.Parse(values[1]);
                            timingPoint.Meter = Int32.Parse(values[2]);
                            timingPoint.SampleType = Int32.Parse(values[3]);
                            timingPoint.SampleSet = Int32.Parse(values[4]);
                            timingPoint.Volume = Int32.Parse(values[5]);
                            timingPoint.Inherited = Int32.Parse(values[6]);
                            timingPoint.KiaiMode = Int32.Parse(values[7]);

                            beatmap.TimingPoints.Add(timingPoint);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    Debug.Log(line);
                    return beatmap;
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

                        if (line.Contains("128"))
                        {
                            string endTime = values[5].Substring(0, values[5].IndexOf(":"));

                            hitObject.X = Int32.Parse(values[0]);

                            // Find which key the object is. TODO: DRY this up.
                            if (hitObject.X >= 0 && hitObject.X <= 127)
                            {
                                hitObject.Key1 = true;
                            }
                            else if (hitObject.X >= 128 && hitObject.X <= 255)
                            {
                                hitObject.Key2 = true;
                            }
                            else if (hitObject.X >= 256 && hitObject.X <= 383)
                            {
                                hitObject.Key3 = true;
                            }
                            else if (hitObject.X >= 384 && hitObject.X <= 511)
                            {
                                hitObject.Key4 = true;
                            }


                            hitObject.Y = Int32.Parse(values[1]);
                            hitObject.StartTime = Int32.Parse(values[2]) + osuOffset;
                            hitObject.Type = Int32.Parse(values[3]);
                            hitObject.HitSound = Int32.Parse(values[4]);
                            hitObject.EndTime = Int32.Parse(endTime) + osuOffset;
                            hitObject.Additions = ":0:0:0:0:";
                        }
                        else
                        {
                            hitObject.X = Int32.Parse(values[0]);

                            // Find which key the object is. TODO: DRY this up.
                            if (hitObject.X >= 0 && hitObject.X <= 127)
                            {
                                hitObject.Key1 = true;
                            }
                            else if (hitObject.X >= 128 && hitObject.X <= 255)
                            {
                                hitObject.Key2 = true;
                            }
                            else if (hitObject.X >= 256 && hitObject.X <= 383)
                            {
                                hitObject.Key3 = true;
                            }
                            else if (hitObject.X >= 384 && hitObject.X <= 511)
                            {
                                hitObject.Key4 = true;
                            }

                            hitObject.Y = Int32.Parse(values[1]);
                            hitObject.StartTime = Int32.Parse(values[2]) + osuOffset;
                            hitObject.Type = Int32.Parse(values[3]);
                            hitObject.HitSound = Int32.Parse(values[4]);
                            hitObject.Additions = "0:0:0:0:";
                        }

                        beatmap.HitObjects.Add(hitObject);
                    }
                }
            }

            return beatmap;
        }
    }
}
