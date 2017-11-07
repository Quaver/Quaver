using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Commands
{
    internal class HelpCommand : ICommand
    {
        public string Name { get; set; } = "HELP";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Lists out all current commands";

        public string Usage { get; set; } = "> help";

        public void Execute(string[] args)
        {
            var commandString = new StringBuilder();
            commandString.AppendLine();
            commandString.AppendLine("The following are a list of commands you can execute:");
            commandString.AppendLine();

            foreach (var cmd in CommandHandler.Commands)
                commandString.AppendLine($"{cmd.Name.ToUpper()} - {cmd.Description}");
            
            Console.WriteLine(commandString);
        }
    }
}
