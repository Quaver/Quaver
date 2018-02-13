using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Sprite;
using Quaver.Logging;

namespace Quaver.Database.Beatmaps
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
        ///     Searches and returns mapsets given a term
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        internal static List<Mapset> SearchMapsets(List<Mapset> mapsets, string term)
        {
            term = term.ToLower();

            return mapsets.Where(x => x.Beatmaps.Any(y => y.Artist.ToLower().Contains(term) || y.Title.ToLower().Contains(term)
                                                          || y.Creator.ToLower().Contains(term) || y.Description.ToLower().Contains(term) ||
                                                          y.Source.ToLower().Contains(term) || y.Tags.ToLower().Contains(term))).ToList();
        }

        /// <summary>
        ///     Used to select a random beatmap from our current list of visible beatmaps.
        /// </summary>
        public static void SelectRandomBeatmap()
        {
            if (GameBase.Mapsets.Count == 0)
                return;

            // Find the number of total beatmaps
            var totalMaps = 0;

            foreach (var mapset in GameBase.Mapsets)
            {
                totalMaps += mapset.Beatmaps.Count;
            }

            var rand = new Random();
            var randomBeatmap = rand.Next(0, totalMaps);

            // Find the totalMaps'th beatmap
            var onMap = 0;
            foreach (var mapset in GameBase.Mapsets)
            {
                var foundBeatmap = false;

                foreach (var beatmap in mapset.Beatmaps)
                {
                    if (onMap == randomBeatmap)
                    {
                        // Switch map and load audio for song and play it.
                        Beatmap.ChangeBeatmap(beatmap);
                        break;
                    }

                    onMap++;
                }

                if (foundBeatmap)
                    break;
            }

            var log = $"RND MAP: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]";
            Logger.LogInfo(log, LogType.Runtime);
        }
    }
}
