using UnityEngine;
using System;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// ParseBeatmap Command. Takes in a file path and parses a beatmap.
    /// </summary>
    public static class ParseBeatmapCommand
    {
        public static readonly string name = "BEATMAP";
        public static readonly string description = "Parses a beatmap and returns data about it | Arguments: (filePath)";
        public static readonly string usage = "BEATMAP";

        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return "ERROR: You need to specify a file path!";
            }

            Beatmap beatmap = BeatmapParser.Parse(String.Join(" ", args));

            if (beatmap.IsValid == false)
            {
                return "ERROR: The specified beatmap could not be found or is not valid.";
            }

            string beatmapLogData = "----------- Beatmap Metadata -----------\n" +
                                    "Valid Beatmap: " + beatmap.IsValid + "\n" +
                                    "Osu File Format: " + beatmap.OsuFileFormat + "\n" +
                                    "AudioFilename: " + beatmap.AudioFilename + "\n" +
                                    "AudioLeadIn: " + beatmap.AudioLeadIn + "\n" +
                                    "PreviewTime: " + beatmap.PreviewTime + "\n" +
                                    "Countdown: " + beatmap.Countdown + "\n" +
                                    "SampleSet: " + beatmap.SampleSet + "\n" +
                                    "StackLeniency: " + beatmap.StackLeniency + "\n" +
                                    "Mode: " + beatmap.Mode + "\n" +
                                    "LetterboxInBreaks: " + beatmap.LetterboxInBreaks + "\n" +
                                    "SpecialStyle: " + beatmap.SpecialStyle + "\n" +
                                    "WidescreenStoryboard: " + beatmap.WidescreenStoryboard;

            return beatmapLogData;
            
        }
    }
}