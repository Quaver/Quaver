// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
                Skin skin = SkinConfigLoader.Load(path);

                string skinLog = "[General] \n" +
                                "Name: " + skin.Name + "\n" +
                                "Author: " + skin.Author + "\n" +
                                "Version: " + skin.Version + "\n";
                return skinLog;
            }

            return "Dude, give me the path of a skin.ini please. You're really chopping my eggplant here.";
        }
    }
}