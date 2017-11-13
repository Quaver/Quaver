using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Beatmaps;

namespace Quaver.Commands
{
    internal class BeatmapSearchCommand : ICommand
    {
        public string Name { get; set; } = "SEARCH";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Find a list of beatmaps that match a given search term";

        public string Usage { get; set; } = "> search <query>";

        public void Execute(string[] args)
        {
            var query = string.Join(" ", args.Skip(1).ToArray());

            var foundMapsets = BeatmapUtils.SearchBeatmaps(GameBase.Beatmaps, query);

            var cmdString = new StringBuilder();
            cmdString.AppendLine();
            cmdString.AppendLine($"Found {foundMapsets.Count} mapsets for search: {query}");
            cmdString.AppendLine();

            var i = 0;
            foreach (var mapset in foundMapsets)
            {
                foreach (var map in mapset.Value)
                {
                    cmdString.AppendLine($"[{i}] {map.Artist} - {map.Title} [{map.DifficultyName}]");
                    i++;
                }
            }

            Console.WriteLine(cmdString);;
        }
    }
}
