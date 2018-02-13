using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Database.Beatmaps;

namespace Quaver.Commands
{
    internal class SearchMaps : ICommand
    {
        public string Name { get; set; } = "SEARCH";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Find a list of beatmaps that match a given search term";

        public string Usage { get; set; } = "> search <query>";

        public void Execute(string[] args)
        {
            var query = string.Join(" ", args.Skip(1).ToArray());

            var foundMapsets = BeatmapUtils.SearchMapsets(GameBase.Mapsets, query);

            Console.WriteLine($"Found {foundMapsets.Count} mapsets");

            for (var i = 0; i < foundMapsets.Count; i++)
            {
                Console.WriteLine($"[{i}] {foundMapsets[i].Beatmaps[0].Artist} - {foundMapsets[i].Beatmaps[0].Title}");
            }
        }
    }
}
