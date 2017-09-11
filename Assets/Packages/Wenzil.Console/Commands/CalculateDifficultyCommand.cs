using UnityEngine;
using System;
using Quaver.Osu.Beatmap;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// ParseBeatmap Command. Takes in a file path and parses a beatmap.
    /// </summary>
    public static class CalculateDifficultyCommand
    {
        public static readonly string name = "DIFFICULTY";
        public static readonly string description = "Calculates a beatmap's difficulty and returns the difficulty values. | Arguments: (filePath)";
        public static readonly string usage = "DIFFICULTY";

        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return "ERROR: You need to specify a file path!";
            }

            OsuBeatmap beatmap = OsuBeatmapParser.Parse(String.Join(" ", args));

            if (beatmap.IsValid == false)
            {
                return "ERROR: The specified beatmap could not be found or is not valid.";
            }

            string beatmapLogData = "----------- Beatmap Metadata -----------\n" +
                                    "Valid Beatmap: " + beatmap.IsValid + "\n";

            return beatmapLogData;
            
        }
    }
}