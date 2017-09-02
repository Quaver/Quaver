using UnityEngine;
using System;
using Qua.Scripts;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// ParseBeatmap Command. Takes in a file path and parses a beatmap.
    /// </summary>
    public static class ParseQuaCommand
    {
        public static readonly string name = "QUA";
        public static readonly string description = "Parses a .qua file and prints the data. | Arguments: (filePath)";
        public static readonly string usage = "QUA";

        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return "ERROR: You need to specify a file path!";
            }

            QuaFile quaFile = QuaParser.Parse(String.Join(" ", args));

            if (quaFile.IsValidQua == false)
            {
                return "ERROR: The specified beatmap could not be found or is not valid.";
            }

			string quaLog = "AudioFile: " + quaFile.AudioFile + "\n" +
							"AudioLeadIn: " + quaFile.AudioLeadIn + "\n" +
							"SongPreviewTime: " + quaFile.SongPreviewTime + "\n" +
							"BackgroundFile: " + quaFile.BackgroundFile + "\n" + 
							"Title: " + quaFile.Title + "\n" + 
							"TitleUnicode: " + quaFile.TitleUnicode + "\n" +
							"Artist: " + quaFile.Artist + "\n" + 
							"ArtistUnicode: " + quaFile.ArtistUnicode + "\n" + 
							"Source: " + quaFile.Source + "\n" + 
							"Tags: " + quaFile.Tags + "\n" + 
							"Creator: " + quaFile.Creator + "\n" + 
							"DifficultyName: " + quaFile.DifficultyName + "\n" + 
							"MapID: " + quaFile.MapID + "\n" + 
							"MapSetID: " + quaFile.MapSetID + "\n" + 
							"HPDrain: " + quaFile.HPDrain + "\n" +
							"AccuracyStrain: " + quaFile.AccuracyStrain + "\n";

			// Add timing points to string
			foreach(TimingPoint timingPoint in quaFile.TimingPoints)
			{
				quaLog += timingPoint.StartTime + "|" + timingPoint.BPM + "\n";
			}

			quaLog += "\n";

			// SVs to string
			foreach(SliderVelocity sv in quaFile.SliderVelocities)
			{
				quaLog += sv.StartTime + "|" + sv.Multiplier + "|" + sv.Volume + "\n";
			}

			quaLog += "\n";

			// HOs to string - lol, hos.
			foreach(HitObject ho in quaFile.HitObjects)
			{
				quaLog += ho.StartTime + "|" + ho.KeyLane + "|" + ho.EndTime + "\n";
			}

			return quaLog;
        }
    }
}