// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Wenzil.Console;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// HELP command. Display the list of available commands or details about a specific command.
    /// </summary>
    public static class HelpCommand
    {
        public static readonly string name = "HELP";
        public static readonly string description = "Display the list of available commands or details about a specific command.";
        public static readonly string usage = "HELP [command]";

        private static StringBuilder s_commandList = new StringBuilder();

        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return DisplayAvailableCommands();
            }
            else
            {
                return DisplayCommandDetails(args[0]);
            }
        }

        private static string DisplayAvailableCommands()
        {
            s_commandList.Length = 0; // clear the command list before rebuilding it
            s_commandList.Append("<b>Available Commands</b>\n");

            foreach (ConsoleCommand command in ConsoleCommandsDatabase.commands)
            {
                s_commandList.Append(string.Format("    <b>{0}</b> - {1}\n", command.name, command.description));
            }

            s_commandList.Append("To display details about a specific command, type 'HELP' followed by the command name.");
            return s_commandList.ToString();
        }

        private static string DisplayCommandDetails(string commandName)
        {
            string formatting =
@"<b>{0} Command</b>
    <b>Description:</b> {1}
    <b>Usage:</b> {2}";

            try
            {
                ConsoleCommand command = ConsoleCommandsDatabase.GetCommand(commandName);
                return string.Format(formatting, command.name, command.description, command.usage);
            }
            catch (NoSuchCommandException exception)
            {
                return string.Format("Cannot find help information about {0}. Are you sure it is a valid command?", exception.command);
            }
        }
    }
}
