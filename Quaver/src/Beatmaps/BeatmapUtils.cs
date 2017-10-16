using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            var logicalOperators = new[] {">", "<", ">=", "<=", "==", "!="};

            // TODO: Break apart the search term if it has any logical operators, and add advanced beatmap searching

            // Find beatmaps by search term.
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
    }
}
