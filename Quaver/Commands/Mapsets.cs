using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Commands
{
    internal class Mapsets : ICommand
    {
        public string Name { get; set; } = "MAPSETS";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Lists all of the loaded beatmap sets";

        public string Usage { get; set; } = "> mapsets";

        public void Execute(string[] args)
        {
            var commandString = new StringBuilder();
            commandString.AppendLine();

            var i = 0;
            foreach (var mapset in GameBase.Mapsets)
            {
                commandString.AppendLine($"[{i}] {mapset.Directory} - {mapset.Beatmaps.Count} maps");
                i++;
            }
            
            Console.WriteLine(commandString);
        }
    }
}
