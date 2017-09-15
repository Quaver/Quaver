// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Quaver.Osu.Beatmap
{
    public static class OsuQuaConverter
    {
        // We want to take an osu! beatmap
        // Parse it to the Beatmap class, then take that data
        // and turn it into a a .qua file
        // We'll return true if the beatmap was successfully converted, false if not.
        public static bool Convert(OsuBeatmap osuBeatmap, string outputName)
        {
            string fileExtension = ".qua";

            if (!outputName.Contains(fileExtension))
            {
                return false;
            }

            // We'll need to build a string in the .qua file format. 
            // For file format details, see Misc/file_format.qua
            StringBuilder fileString = new StringBuilder();

            // # General
            fileString.Append("# General\n");
            fileString.Append("AudioFile: " + osuBeatmap.AudioFilename + "\n");
            fileString.Append("AudioLeadIn: " + osuBeatmap.AudioLeadIn + "\n");
            fileString.Append("SongPreviewTime: " + osuBeatmap.PreviewTime + "\n");
            fileString.Append("BackgroundFile: " + osuBeatmap.Background + "\n\n");

            // # Metadata
            fileString.Append("# Metadata\n");
            fileString.Append("Title: " + osuBeatmap.Title + "\n");
            fileString.Append("TitleUnicode: " + osuBeatmap.TitleUnicode + "\n");
            fileString.Append("Artist: " + osuBeatmap.Artist + "\n");
            fileString.Append("ArtistUnicode: " + osuBeatmap.ArtistUnicode + "\n");
            fileString.Append("Source: " + osuBeatmap.Source + "\n");
            fileString.Append("Tags: " + osuBeatmap.Tags + "\n");
            fileString.Append("Creator: " + "\n");
            fileString.Append("DifficultyName: " + osuBeatmap.Version + "\n");
            fileString.Append("MapID: -1\n");
            fileString.Append("MapSetID: -1\n");
            fileString.Append("Description: This beatmap was converted from osu!mania.\n\n");

            // # Difficulty
            fileString.Append("# Difficulty\n");
            fileString.Append("HPDrain: " + osuBeatmap.HPDrainRate + "\n");
            fileString.Append("AccuracyStrain: " + osuBeatmap.OverallDifficulty + "\n\n");

            // # Timing (StartTime | BPM)
            fileString.Append("# Timing\n");
            foreach (TimingPoint timingPoint in osuBeatmap.TimingPoints)
            {
                // Find all of the red lined timing points and add their data to the string.
                if (timingPoint.Inherited == 1)
                {
                    fileString.Append(timingPoint.Offset + "|" + 60000 / timingPoint.MillisecondsPerBeat + "\n");
                }
            }

            // # SV (StartTime|Multiplier|Volume)
            fileString.Append("\n# SV\n");
            foreach (TimingPoint timingPoint in osuBeatmap.TimingPoints)
            {
                if (timingPoint.Inherited == 0)
                {
                    // The formula to get the SV Multiplier: 0.10 / ((MilliSecondsPerBeat / -100) / 10) - Jesus christ... MATH
                    fileString.Append(timingPoint.Offset + "|" + Math.Round(0.10 / ((timingPoint.MillisecondsPerBeat / -100) / 10), 2) + "|" + timingPoint.Volume + "\n");
                }
            }

            // # HitObject (StartTime|KeyLane|EndTime)
            fileString.Append("\n# HitObjects\n");
            foreach (HitObject hitObject in osuBeatmap.HitObjects)
            {
                // Get the keyLane the hitObject is in
                int keyLane = 0;

                if (hitObject.Key1)
                    keyLane = 1;
                else if (hitObject.Key2)
                    keyLane = 2;
                else if (hitObject.Key3)
                    keyLane = 3;
                else if (hitObject.Key4)
                    keyLane = 4;

                // Normal HitObjects
                if (hitObject.Type == 1 || hitObject.Type == 5)
                {
                    fileString.Append(hitObject.StartTime + "|" + keyLane + "|" + "0\n");
                }

                // LNs
                else if (hitObject.Type == 128 || hitObject.Type == 22)
                {
                    fileString.Append(hitObject.StartTime + "|" + keyLane + "|" + hitObject.EndTime + "\n");
                }
            }

            try
            {
                StreamWriter file = new StreamWriter(outputName);
                file.AutoFlush = true;
                Debug.Log(fileString.ToString());
                file.WriteLine(fileString.ToString());

                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return false;
            }
        }
    }
}
