using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Quaver.Logging;

namespace Quaver.Beatmaps
{
    internal static class BeatmapUtils
    {
        /// <summary>
        ///     Gets the Md5 Checksum of a file, more specifically a .qua file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetMd5Checksum(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty).ToLower();
                }
            }
        }

        /// <summary>
        ///     Responsible for taking a list of beatmaps, and grouping each directory.
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        internal static List<Mapset> GroupBeatmapsByDirectory(List<Beatmap> beatmaps)
        {
            // Group maps by directory.
            var groupedMaps = beatmaps
                .GroupBy(u => u.Directory)
                .Select(grp => grp.ToList())
                .ToList();

            // Populate the mapsets with the grouped maps.
            var mapsets = new List<Mapset>();

            foreach (var mapset in groupedMaps)
                mapsets.Add(new Mapset() { Directory = mapset.First().Directory, Beatmaps = mapset });

            return mapsets;
        }

        /// <summary>
        ///     Orders the beatmaps by title
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderBeatmapsByTitle(List<Mapset> beatmaps)
        {
            return beatmaps.OrderBy(x => x.Directory).ToList();
        }

        /// <summary>
        ///     Orders the beatmaps by artist, and then by title.
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderBeatmapsByArtist(List<Mapset> beatmaps)
        {
            return beatmaps.OrderBy(x => x.Beatmaps[0].Artist).ThenBy(x => x.Beatmaps[0].Title).ToList();
        }

        /// <summary>
        ///     Searches and returns beatmaps 
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        internal static Dictionary<string, List<Beatmap>> SearchBeatmaps(Dictionary<string, List<Beatmap>> beatmaps, string term)
        {
            // Lowercase which ever term comes in.
            term = term.ToLower();

            // Stores the new dictionary of found maps.
            var foundMaps = new Dictionary<string, List<Beatmap>>();

            // All the possible relational operators for our search terms
            var relationalOperators = new[] {">", "<", "=", "=="};
            var searchOptions = new[] { "bpm", "stars", "status", "length" };

            // TODO: Break apart the search term if it has any relational operators, and add advanced beatmap searching
            foreach (var relationalOperator in relationalOperators)
            {
                // If the search query does in deed have a relational operator, get the word string before it and after.
                if (term.Contains(relationalOperator))
                {
                    // Get the search option alone.
                    var searchOption = term.Substring(0, term.IndexOf(relationalOperator));
                    searchOption = searchOption.Split(' ').Last().ToLower();

                    // Get the search query alone.
                    var query = term.Substring(term.IndexOf(relationalOperator) + 1);
                    query = query.Split(' ').First().ToLower();

                    // Try to parse the query number.
                    var queryNum = 0;
                    try
                    {
                        queryNum = Int32.Parse(query);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }

                    // Go through each relational operator 
                    switch (relationalOperator)
                    {
                        case ">":
                            switch (searchOption)
                            {
                                case "bpm":
                                    foreach (var set in beatmaps)
                                    {
                                        var maps = set.Value.FindAll(x => x.Bpm > queryNum);

                                        if (maps.Count > 0)
                                            foundMaps.Add(Path.GetDirectoryName(maps[0].Path), maps);
                                    }
                                    break;
                            }
                            break;
                        case "<":
                            switch (searchOption)
                            {
                                case "bpm":
                                    foreach (var set in beatmaps)
                                    {
                                        var maps = set.Value.FindAll(x => x.Bpm < queryNum);

                                        if (maps.Count > 0)
                                            foundMaps.Add(Path.GetDirectoryName(maps[0].Path), maps);
                                    }
                                    break;
                            }
                            break;
                        case "=":
                            switch (searchOption)
                            {
                                case "bpm":
                                    foreach (var set in beatmaps)
                                    {
                                        var maps = set.Value.FindAll(x => Convert.ToInt32(x.Bpm) == queryNum);

                                        if (maps.Count > 0)
                                            foundMaps.Add(Path.GetDirectoryName(maps[0].Path), maps);
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                }
            }

            // If we've already found maps from the search query, just return without caring about the results afterward.
            if (foundMaps.Values.Count > 0)
                return foundMaps;

            // Find beatmaps by search term if the search query doesn't have relational operators.
            foreach (var beatmapSet in beatmaps)
            {
                var maps = beatmapSet.Value.FindAll(x => x.Artist.ToLower().Contains(term) || x.Title.ToLower().Contains(term) ||
                                                        x.Creator.ToLower().Contains(term) || x.DifficultyName.ToLower().Contains(term) ||
                                                        x.Tags.ToLower().Contains(term) || x.Description.Contains(term) ||
                                                        x.Source.ToLower().Contains(term));

                // Add add the beatmaps to the dictionary.
                if (maps.Count > 0)
                    foundMaps.Add(Path.GetDirectoryName(maps[0].Path), maps);
            }

            return foundMaps;
        }

        /// <summary>
        ///     Used to select a random beatmap from our current list of visible beatmaps.
        /// </summary>
        public static void SelectRandomBeatmap()
        {
            if (GameBase.Beatmaps.Count == 0)
                return;

            // Find the number of total beatmaps
            var totalMaps = 0;

            foreach (var mapset in GameBase.Beatmaps)
            {
                totalMaps += mapset.Beatmaps.Count;
            }

            var rand = new Random();
            var randomBeatmap = rand.Next(0, totalMaps);

            // Find the totalMaps'th beatmap
            var onMap = 0;
            foreach (var mapset in GameBase.Beatmaps)
            {
                var foundBeatmap = false;

                foreach (var beatmap in mapset.Beatmaps)
                {
                    if (onMap == randomBeatmap)
                    {
                        if (GameBase.CurrentBackground != null)
                            GameBase.CurrentBackground.Dispose();

                        GameBase.SelectedBeatmap = null;
                        GameBase.SelectedBeatmap = beatmap;
                        foundBeatmap = true;
                        break;
                    }

                    onMap++;
                }

                if (foundBeatmap)
                    break;
            }

            Logger.Log($"Random Beatmap Selected: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]", Color.Cyan);
        }
    }
}
