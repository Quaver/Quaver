using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;

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
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
                }
            }
        }

        /// <summary>
        ///     Responsible for taking a list of beatmaps, and grouping each directory into a dictionary.
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        internal static Dictionary<string, List<Beatmap>> GroupBeatmapsByDirectory(List<Beatmap> beatmaps)
        {
            return (from beatmap in beatmaps
                    group beatmap by Path.GetDirectoryName(beatmap.Path)
                into g
                    select g).ToDictionary(x => x.Key, x => x.ToList());
        }

        /// <summary>
        ///     Orders the beatmaps by title
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        internal static Dictionary<string, List<Beatmap>> OrderBeatmapsByTitle(Dictionary<string, List<Beatmap>> beatmaps)
        {
            return beatmaps.OrderBy(x => x.Value[0].Title).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        ///     Orders the beatmaps by artist, and then by title.
        /// </summary>
        /// <param name="beatmaps"></param>
        /// <returns></returns>
        internal static Dictionary<string, List<Beatmap>> OrderBeatmapsByArtist(Dictionary<string, List<Beatmap>> beatmaps)
        {
            var dict = beatmaps.OrderBy(x => x.Value[0].Artist).ThenBy(x => x.Value[0].Title)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            return dict;
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

            // All the possible logical operators for our search terms
            var logicalOperators = new[] {">", "<", "=", "=="};
            var searchOptions = new[] { "bpm", "stars", "status", "length" };

            // TODO: Break apart the search term if it has any logical operators, and add advanced beatmap searching
            foreach (var logicalOperator in logicalOperators)
            {
                // If the search query does in deed have a logical operator, get the word string before it and after.
                if (term.Contains(logicalOperator))
                {
                    // Get the search option alone.
                    var searchOption = term.Substring(0, term.IndexOf(logicalOperator));
                    searchOption = searchOption.Split(' ').Last().ToLower();

                    // Get the search query alone.
                    var query = term.Substring(term.IndexOf(logicalOperator) + 1);
                    query = query.Split(' ').First().ToLower();

                    // Try to parse the query number.
                    var queryNum = 0;
                    try
                    {
                        queryNum = int.Parse(query);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }

                    // Go through each logical operator 
                    switch (logicalOperator)
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
                    Console.WriteLine($"Found Mapsets: {foundMaps.Values.Count}");
                    break;
                }
            }

            // If we've already found maps from the search query, just return without caring about the results afterward.
            if (foundMaps.Values.Count > 0)
                return foundMaps;

            // Find beatmaps by search term if the search query doesn't have logical operators.
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

            Console.WriteLine($"Found Mapsets: {foundMaps.Values.Count}");
            return foundMaps;
        }
    }
}
