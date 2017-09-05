using UnityEngine;
using System;
using Config.Scripts;

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
				return "Config file was successfully generated at: " + ConfigDefault.GameDirectory + "/quaver.cfg";
			}

			return "File could not be generated.";
            
        }
    }
}