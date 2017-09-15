// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using System;
using Quaver.Osu.Beatmap;

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

            OsuBeatmap osuBeatmap = OsuBeatmapParser.Parse(args[0]);

            if (!osuBeatmap.IsValid)
            {
                return "Invalid osu! beatmap specified. Please give a correct beatmap.";
            }

            bool hasConverted = OsuQuaConverter.Convert(osuBeatmap, args[1]);

            if (hasConverted)
            {
                return "Beatmap successfully converted @ " + args[1];
            }

            return "ERROR: The beatmap failed to convert.";
        }
    }
}