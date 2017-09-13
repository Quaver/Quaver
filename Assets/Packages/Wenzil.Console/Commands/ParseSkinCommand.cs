using UnityEngine;
using System;
using Quaver.Skin;
using IniParser.Model;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// PING Command. A test command to ping the console.
    /// </summary>
    public static class ParseSkinCommand
    {
        public static readonly string name = "SKIN";
        public static readonly string description = "Parses an .ini file used for a skin. | Args: (name)";
        public static readonly string usage = "SKIN";

        public static string Execute(params string[] args)
        {
			if (args.Length > 0) 
			{
				string path = String.Join(" ", args);
				IniData skin = SkinConfigLoader.Load(path);

				Debug.Log(skin["General"]["Name"]);
				return "Done!";
			}

			return "Dude, give me the path of a skin.ini please. You're really chopping my eggplant here.";
        }
    }
}