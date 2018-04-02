using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Database.Beatmaps;
using Quaver.Main;

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

            var foundMapsets = BeatmapHelper.SearchMapsets(GameBase.Mapsets, query);

            Console.WriteLine($"Found {foundMapsets.Count} mapsets");

            for (var i = 0; i < foundMapsets.Count; i++)
            {
                Console.WriteLine($"[Mapset: {i}] {foundMapsets[i].Directory}");

                for (var j = 0; j < foundMapsets[i].Beatmaps.Count; j++)
                    Console.WriteLine($"\t[Map: {j}] {foundMapsets[i].Beatmaps[j].Artist} - {foundMapsets[i].Beatmaps[j].Title} [{foundMapsets[i].Beatmaps[j].DifficultyName}]");
            }
        }
    }
}
