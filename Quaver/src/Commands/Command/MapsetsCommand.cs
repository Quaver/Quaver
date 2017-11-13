using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Commands
{
    internal class MapsetsCommand : ICommand
    {
        public string Name { get; set; } = "MAPSETS";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Lists all of the loaded beatmap sets";

        public string Usage { get; set; } = "> mapsets";

        public void Execute(string[] args)
        {
            var commandString = new StringBuilder();
            commandString.AppendLine();

            //  It's a dictonary where the keys are strings, so please.
            var i = 0;
            foreach (var mapset in GameBase.Beatmaps)
            {
                commandString.AppendLine($"[{i}] {new DirectoryInfo(mapset.Key).Name}  - {mapset.Value.Count} maps");
                i++;
            }
            
            Console.WriteLine(commandString);
        }
    }
}
