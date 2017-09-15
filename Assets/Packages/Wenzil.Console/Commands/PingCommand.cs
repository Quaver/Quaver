// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using UnityEngine;
using System;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// PING Command. A test command to ping the console.
    /// </summary>
    public static class PingCommand
    {
        public static readonly string name = "PING";
        public static readonly string description = "Ping the developer console. | Arguments: (name)";
        public static readonly string usage = "PING";

        public static string Execute(params string[] args)
        {
            if (args.Length > 0)
            {
                string name = String.Join(" ", args);
                return "Hello, " + name + ". Welcome to the developer console. Use the command HELP for a list of commands.";
            }

            return "Welcome to the developer console, friend. Use the command HELP for a list of commands.";
        }
    }
}