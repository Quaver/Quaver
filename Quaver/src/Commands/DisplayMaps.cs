using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Commands
{
    internal class DisplayMaps : ICommand
    {
        public string Name { get; set; } = "DISPLAYMAPS";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Lists all of the currently loaded beatmaps.";

        public string Usage { get; set; } = "> maps";

        public void Execute(string[] args)
        {
            var commandString = new StringBuilder();
            commandString.AppendLine();

            var i = 0;
            foreach (var mapset in GameBase.Beatmaps)
            {
                foreach (var beatmap in mapset.Beatmaps)
                {
                    commandString.AppendLine($"[{i}] {beatmap.Artist} - {beatmap.Title} [{beatmap.DifficultyName}]");
                    i++;
                }
            }

            Console.WriteLine(commandString);
        }
    }
}
