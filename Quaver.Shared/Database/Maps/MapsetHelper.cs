/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Wobble.Logging;

namespace Quaver.Shared.Database.Maps
{
    public static class MapsetHelper
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
        ///     Responsible for taking a list of maps, and grouping each directory.
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        internal static List<Mapset> ConvertMapsToMapsets(IEnumerable<Map> maps)
        {
            // Group maps by directory.
            var groupedMaps = maps
                .GroupBy(u => u.Directory)
                .Select(grp => grp.ToList())
                .ToList();

            // Populate the mapsets with the grouped maps.
            var mapsets = new List<Mapset>();

            foreach (var mapset in groupedMaps)
            {
                var set = new Mapset()
                {
                    Directory = mapset.First().Directory,
                    Maps = mapset
                };

                set.Maps.ForEach(x => x.Mapset = set);
                mapsets.Add(set);
            }

            return mapsets;
        }

        /// <summary>
        ///     Orders the mapsets by artist, and then by title.
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByArtist(IEnumerable<Mapset> mapsets)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            return mapsets.OrderBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders mapsets by title.
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByTitle(IEnumerable<Mapset> mapsets) => mapsets.OrderBy(x => x.Maps.First().Title).ToList();

        /// <summary>
        ///     Orders mapsets by creator.
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByCreator(IEnumerable<Mapset> mapsets)
        {
            return mapsets.OrderBy(x => x.Maps.First().Creator).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders the mapsets based on the set config value.
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static List<Mapset> OrderMapsetsByConfigValue(IEnumerable<Mapset> mapsets)
        {
            switch (ConfigManager.SelectOrderMapsetsBy.Value)
            {
                case OrderMapsetsBy.Artist:
                    return OrderMapsetsByArtist(mapsets);
                case OrderMapsetsBy.Title:
                    return OrderMapsetsByTitle(mapsets);
                case OrderMapsetsBy.Creator:
                    return OrderMapsetsByCreator(mapsets);
                case OrderMapsetsBy.DateAdded:
                    return OrderMapsetsByDateAdded(mapsets);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        ///     Orders the map's mapsets by date added
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByDateAdded(IEnumerable<Mapset> mapsets)
            => mapsets.OrderByDescending(x => x.Maps.First().DateAdded).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();

        /// <summary>
        ///     Orders the map's mapsets by difficulty.
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsByDifficulty(List<Mapset> mapsets)
        {
            mapsets.ForEach(x => x.Maps = x.Maps.OrderBy(y => y.Difficulty10X).ToList());
            return mapsets;
        }

        /// <summary>
        /// Searches and returns mapsets given a term
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        internal static List<Mapset> SearchMapsets(IEnumerable<Mapset> mapsets, string term)
        {
            var sets = new List<Mapset>();

            term = term.ToLower();

            // All the possible relational operators for the search query
            var operators = new List<string>
            {
                ">=",
                "<=",
                "==",
                "!=",
                "<",
                ">",
                "=",
            };
            var options = new List<string>
            {
                "bpm",
                "diff",
                "length",
                "keys",
                "status"
            };

            // Stores a dictionary of the found pairs in the search query
            // <option, value, operator>
            var foundSearchQueries = new List<SearchQuery>();

            // Get a list of all the matching search queries
            foreach (var op in operators)
            {
                if (!Regex.Match(term, $@"\b{op}\b", RegexOptions.IgnoreCase).Success)
                    continue;

                // Get the search option alone.
                var searchOption = term.Substring(0, term.IndexOf(op, StringComparison.InvariantCultureIgnoreCase))
                    .Split(' ').Last();

                var val = term.Substring(term.IndexOf(op, StringComparison.InvariantCultureIgnoreCase) + op.Length).Split(' ')
                         .First();

                if (options.Contains(searchOption))
                    foundSearchQueries.Add(new SearchQuery
                    {
                        Operator = op,
                        Option = searchOption,
                        Value = val
                    });
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
                                if (!float.TryParse(searchQuery.Value, out var valBpm))
                                    exitLoop = true;

                                if (!CompareValues(map.Bpm, valBpm, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case "diff":
                                if (!float.TryParse(searchQuery.Value, out var valDiff))
                                    exitLoop = true;

                                if (!CompareValues(map.Difficulty10X, valDiff, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case "length":
                                if (!float.TryParse(searchQuery.Value, out var valLength))
                                    exitLoop = true;

                                if (!CompareValues(map.SongLength, valLength, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case "keys":
                                switch (map.Mode)
                                {
                                    case GameMode.Keys4:
                                        if (!float.TryParse(searchQuery.Value, out var val4k))
                                            exitLoop = true;

                                        if (!CompareValues(4, val4k, searchQuery.Operator))
                                            exitLoop = true;
                                        break;
                                    case GameMode.Keys7:
                                        if (!float.TryParse(searchQuery.Value, out var val7k))
                                            exitLoop = true;

                                        if (!CompareValues(7, val7k, searchQuery.Operator))
                                            exitLoop = true;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                                break;
                            case "status":
                                if (!(searchQuery.Operator.Equals(operators[2]) ||
                                    searchQuery.Operator.Equals(operators[6])))
                                    exitLoop = true;

                                switch (map.RankedStatus)
                                {
                                    case RankedStatus.DanCourse:
                                        if (!CompareValues("dan", searchQuery.Value, searchQuery.Operator))
                                            exitLoop = true;
                                        break;
                                    case RankedStatus.NotSubmitted:
                                        if (!CompareValues("notsubmitted", searchQuery.Value, searchQuery.Operator))
                                            exitLoop = true;
                                        break;
                                    case RankedStatus.Ranked:
                                        if (!CompareValues("ranked", searchQuery.Value, searchQuery.Operator))
                                            exitLoop = true;
                                        break;
                                    case RankedStatus.Unranked:
                                        if (!CompareValues("unranked", searchQuery.Value, searchQuery.Operator))
                                            exitLoop = true;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
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
                    term = foundSearchQueries.Aggregate(term,
                        (current, query) => current.Replace(query.Option + query.Operator + query.Value, "")).Trim();

                    // Check if the term exist in any of the following properties
                    try
                    {
                        if (!map.Artist.ToLower().Contains(term) && !map.Title.ToLower().Contains(term) &&
                            !map.Creator.ToLower().Contains(term) && !map.Source.ToLower().Contains(term) &&
                            !map.Description.ToLower().Contains(term) && !map.Tags.ToLower().Contains(term) &&
                            !map.DifficultyName.ToLower().Contains(term))
                            continue;
                    }
                    catch (Exception)
                    {
                        // Some values can be null and they break the game.
                        continue;
                    }

                    // Add the set if all the comparisons and queries are correct
                    if (sets.All(x => x.Directory != map.Directory))
                        sets.Add(new Mapset()
                        {
                            Directory = map.Directory,
                            Maps = new List<Map> {map}
                        });
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
            if (val1 == null || val2 == null)
                return false;

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
    }

    /// <summary>
    ///     A struct for the map searching method.
    /// </summary>
    public struct SearchQuery
    {
        /// <summary>
        ///     The search option - bpm, length, etc,
        /// </summary>
        public string Option;

        /// <summary>
        ///     The value the user is searching
        /// </summary>
        public string Value;

        /// <summary>
        ///     The operator the user gave
        /// </summary>
        public string Operator;
    }
}
