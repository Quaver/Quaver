using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Commands
{
    internal class CommandHandler
    {
        /// <summary>
        ///     Stores all of the game's commands.
        /// </summary>
        public static ICommand[] Commands { get; set; } =
        {
            new HelpCommand(),
            new MapsetsCommand(),
            new BeatmapsCommand()
        };

        /// <summary>
        ///     Executes a given command if it exists
        /// </summary>
        internal static void Execute(string input)
        {
            var args = input.Split(' ');
            var command = args[0];

            // Check if the command exists, and if the args are correct
            foreach (var cmd in Commands)
            {
                if (!command.ToLower().Equals(cmd.Name.ToLower()))
                    continue;

                cmd.Execute();
                return;
            }
                    
            Console.WriteLine("Command not found.");
        }
    }
}
