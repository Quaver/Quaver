using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Main;

namespace Quaver.Commands
{
    internal class ListMapsetsCommand : ICommand
    {
        public string Name { get; set; } = "LISTMAPSETS";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Lists all of the loaded beatmap sets";

        public string Usage { get; set; } = "> listmapsets";

        public void Execute()
        {
            var commandString = new StringBuilder();

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
