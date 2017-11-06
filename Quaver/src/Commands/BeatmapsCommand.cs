using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Main;

namespace Quaver.Commands
{
    internal class BeatmapsCommand : ICommand
    {
        public string Name { get; set; } = "BEATMAPS";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Lists all of the currently loaded beatmaps.";

        public string Usage { get; set; } = "> beatmaps";

        public void Execute()
        {
            var commandString = new StringBuilder();
            commandString.AppendLine();

            //  It's a dictonary where the keys are strings, so please.
            var i = 0;
            foreach (var mapset in GameBase.Beatmaps)
            {
                foreach (var beatmap in mapset.Value)
                {
                    commandString.AppendLine($"[{i}] {beatmap.Artist} - {beatmap.Title} [{beatmap.DifficultyName}]");
                    i++;
                }
            }
            
            Console.WriteLine(commandString);
        }
    }
}
