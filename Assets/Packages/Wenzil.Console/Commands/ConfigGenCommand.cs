// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using System;
using Quaver.Config;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// ParseBeatmap Command. Takes in a file path and parses a beatmap.
    /// </summary>
    public static class ConfigGenCommand
    {
        public static readonly string name = "GENCONFIG";
        public static readonly string description = "Creates a default Quaver configuration file on your machine.";
        public static readonly string usage = "GENCONFIG";

        public static string Execute(params string[] args)
        {
            bool hasGenerated = ConfigGenerator.Generate();

            if (hasGenerated)
            {
                return "Config file was successfully generated at: " + ConfigDefault.ConfigDirectory;
            }

            return "File could not be generated.";
        }
    }
}