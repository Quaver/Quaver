using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.Database.Maps
{
    internal static class MapsetHelper
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
        ///     Responsible for taking a list of maps, and grouping each directory.
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        internal static List<Mapset> ConvertMapsToMapsets(List<Map> maps)
        {
            // Group maps by directory.
            var groupedMaps = maps
                .GroupBy(u => u.Directory)
                .Select(grp => grp.ToList())
                .ToList();

            // Populate the mapsets with the grouped maps.
            var mapsets = new List<Mapset>();

            foreach (var mapset in groupedMaps)
                mapsets.Add(new Mapset() { Directory = mapset.First().Directory, Maps = mapset });

            return mapsets;
        }

        /// <summary>
        ///     Orders the mapsets by title
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByTitle(List<Mapset> mapsets)
        {
            return mapsets.OrderBy(x => x.Directory).ToList();
        }

        /// <summary>
        ///     Orders the mapsets by artist, and then by title.
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByArtist(List<Mapset> mapsets)
        {
            return mapsets.OrderBy(x => x.Maps[0].Artist).ThenBy(x => x.Maps[0].Title).ToList();
        }

        /// <summary>
        ///     Orders the map's mapsets by difficulty.
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByDifficulty(List<Mapset> mapsets)
        {
            mapsets.ForEach(x => x.Maps = x.Maps.OrderBy(y => y.DifficultyRating).ToList());
            return mapsets;
        }

        /// <summary>
        /// Searches and returns mapsets given a term
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        internal static List<Mapset> SearchMapsets(List<Mapset> mapsets, string term)
        {
            var sets = new List<Mapset>();

            term = term.ToLower();

            // All the possible relational operators for the search query
            var operators = new List<string> {">=", "<=", "==", "!=", "<", ">", "=",};
            var options = new List<string> {"bpm", "diff", "length"};

            // Stores a dictionary of the found pairs in the search query
            // <option, value, operator>
            var foundSearchQueries = new List<SearchQuery>();

            // Get a list of all the matching search queries
            foreach (var op in operators)
            {
                if (!Regex.Match(term, $@"\b{op}\b", RegexOptions.IgnoreCase).Success)
                    continue;

                // Get the search option alone.
                var searchOption = term.Substring(0, term.IndexOf(op, StringComparison.InvariantCultureIgnoreCase)).Split(' ').Last();
                float.TryParse(term.Substring(term.IndexOf(op, StringComparison.InvariantCultureIgnoreCase) + op.Length).Split(' ').First(), out var val);

                if (options.Contains(searchOption))
                   foundSearchQueries.Add(new SearchQuery() { Operator = op, Option = searchOption, Value = val });             
            }

            // Create a list of mapsets with the matched mapsets
            foreach (var mapset in mapsets)
            {
                foreach (var map in mapset.Maps)
                {
                    var exitLoop = false;

                    foreach (var searchQuery in foundSearchQueries)
                    {

                        switch (searchQuery.Option)
                        {
                            case "bpm":
                                if (!CompareValues(map.Bpm, searchQuery.Value, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case "diff":
                                if (!CompareValues(map.DifficultyRating, searchQuery.Value, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case "length":
                                if (!CompareValues(map.SongLength, searchQuery.Value, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            default:
                                break;
                        }

                        if (exitLoop)
                            break;
                    }

                    if (exitLoop)
                        continue;

                    // Find the parts of the original query that aren't operators
                    term = foundSearchQueries.Aggregate(term, (current, query) => current.Replace(query.Option + query.Operator + query.Value, "")).Trim();

                    // Check if the term exist in any of the following properties
                    if (!map.Artist.ToLower().Contains(term) && !map.Title.ToLower().Contains(term) &&
                        !map.Creator.ToLower().Contains(term) && !map.Source.ToLower().Contains(term) && 
                        !map.Description.ToLower().Contains(term) && !map.Tags.ToLower().Contains(term))
                        continue;

                    // Add the set if all the comparisons and queries are correct
                    if (sets.All(x => x.Directory != map.Directory))
                        sets.Add(new Mapset() { Directory = map.Directory, Maps = new List<Map> { map } });
                    else
                        sets.Find(x => x.Directory == map.Directory).Maps.Add(map);
                }
            }

            return sets;
        }

        /// <summary>
        ///     Compares two values and determines 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private static bool CompareValues<T>(T val1, T val2, string operation) where T : IComparable<T>
        {
            var compared = val1.CompareTo(val2);

            switch (operation)
            {
                case "<":
                    return compared < 0;
                case ">":
                    return compared > 0;
                case "=":
                case "==":
                    return compared == 0;
                case "<=":
                    return compared <= 0;
                case ">=":
                    return compared >= 0;
                case "!=":
                    return compared != 0;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Used to select a random map from our current list of visible maps.
        /// </summary>
        public static void SelectRandomMap()
        {
            if (GameBase.Mapsets.Count == 0)
                return;

            // Find the number of total maps
            var totalMaps = 0;

            foreach (var mapset in GameBase.Mapsets)
            {
                totalMaps += mapset.Maps.Count;
            }

            var rand = new Random();
            var randomMap = rand.Next(0, totalMaps);

            // Find the totalMaps'th map
            var onMap = 0;
            foreach (var mapset in GameBase.Mapsets)
            {
                var foundMap = false;

                foreach (var map in mapset.Maps)
                {
                    if (onMap == randomMap)
                    {
                        // Switch map and load audio for song and play it.
                        Map.ChangeSelected(map);
                        break;
                    }

                    onMap++;
                }

                if (foundMap)
                    break;
            }

            var log = $"RND MAP: {GameBase.SelectedMap.Artist} - {GameBase.SelectedMap.Title} [{GameBase.SelectedMap.DifficultyName}]";
            Logger.LogInfo(log, LogType.Runtime);
        }
    }

    /// <summary>
    ///     A struct for the map searching method.
    /// </summary>
    public struct SearchQuery
    {
        /// <summary>
        ///     The search option - bpm, length, etc,
        /// </summary>
        internal string Option;

        /// <summary>
        ///     The value the user is searching
        /// </summary>
        internal float Value;

        /// <summary>
        ///     The operator the user gave
        /// </summary>
        internal string Operator;
    }
}
