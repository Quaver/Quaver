using UnityEngine;
using System;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// ParseBeatmap Command. Takes in a file path and parses a beatmap.
    /// </summary>
    public static class ParseBeatmapCommand
    {
        public static readonly string name = "PARSEBEATMAP";
        public static readonly string description = "Parses a beatmap and returns data about it | Arguments: (filePath)";
        public static readonly string usage = "PARSEBEATMAP";

        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return "ERROR: You need to specify a file path!";
            }

            Beatmap beatmap = BeatmapParser.Parse(args[0]);

            string beatmapLogData = "----------- Beatmap Metadata -----------\n" +
                                "Beatmap Valid: " + beatmap.valid + "\n" +
                                "Title: " + beatmap.title + "\n" +
                                "Subtitle: " + beatmap.subtitle + "\n" +
                                "Artist: " + beatmap.artist + "\n" +
                                "Banner: " + beatmap.bannerPath + "\n" +
                                "Background: " + beatmap.backgroundPath + "\n" +
                                "Music: " + beatmap.musicPath + "\n" +
                                "Offset:" + beatmap.offset + "\n" +
                                "Sample Start: " + beatmap.sampleStart + "\n" +
                                "Sample Length: " + beatmap.sampleLength + "\n\n" +

                                // Display BPM Data. TODO: Parse multiple bpm changes, right now we only get the first
                                // offset and bpm. See bpms.cs & the BPMS case in the switch statement above.
                                "----------- BPM Data -----------\n" +
                                "[0] Offset: " + beatmap.bpms[0].offset + " | BPM: " + beatmap.bpms[0].bpm + "\n";

            return beatmapLogData;
        }
    }
}