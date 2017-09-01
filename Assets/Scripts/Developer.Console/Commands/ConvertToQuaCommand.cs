using UnityEngine;
using System;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// ConvertToQua Command. Converts an .osu file to a .qua file
    /// </summary>
    public static class ConvertToQuaCommand
    {
        public static readonly string name = "CVQ";
        public static readonly string description = "Converts an .osu beatmap to a .qua file | Args: (.osu file) (outputFile.qua)";
        public static readonly string usage = "CVQ";

        public static string Execute(params string[] args)
        {
            if (args.Length < 2) 
            {
                return "Invalid arguments passed. Usage: cvq ./cool_beatmap.osu cooler_beatmap.qua";
            }

			Beatmap osuBeatmap = OsuBeatmapParser.Parse(args[0]);

            if (!osuBeatmap.IsValid)
            {
                return "Invalid osu! beatmap specified. Please give a correct beatmap.";
            }

            bool hasConverted = OsuToQua.Convert(osuBeatmap, args[1]);

            if (hasConverted) 
            {
                return "Beatmap successfully converted @ " + args[1];
            } 

            return "ERROR: The beatmap failed to convert.";
        }
    }
}