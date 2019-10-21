/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;

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
        ///     Filters mapsets to use in song select
        /// </summary>
        /// <returns></returns>
        internal static List<Mapset> FilterMapsets(Bindable<string> searchQuery, bool musicPlayer = false)
        {
            var mapsets = MapManager.Mapsets;

            // Handle playlists
            if (ConfigManager.SelectGroupMapsetsBy.Value == GroupMapsetsBy.Playlists)
            {
                mapsets = PlaylistManager.Selected.Value == null
                    ? new List<Mapset>()
                    : SeparateMapsIntoOwnMapsets(PlaylistManager.Selected.Value.Maps);
            }

            var searched = SearchMapsets(mapsets, searchQuery.Value);

            if (ConfigManager.SelectGroupMapsetsBy.Value == GroupMapsetsBy.Playlists)
                searched = SeparateMapsIntoOwnMapsets(searched);

            var separateMapsets = ConfigManager.SelectGroupMapsetsBy.Value != GroupMapsetsBy.Playlists;
            var gameModes = musicPlayer ? new Bindable<SelectFilterGameMode>(SelectFilterGameMode.All) {Value = SelectFilterGameMode.All} : null;
            var orderMapsetsBy = musicPlayer ? ConfigManager.MusicPlayerOrderMapsBy : null;

            return OrderMapsetsByConfigValue(searched, separateMapsets, gameModes, orderMapsetsBy);
        }

        /// <summary>
        ///     Returns if in song select, we're sorting by single difficulties
        /// </summary>
        /// <returns></returns>
        internal static bool IsSingleDifficultySorted()
        {
            if (ConfigManager.SelectGroupMapsetsBy.Value == GroupMapsetsBy.Playlists)
                return true;

            switch (ConfigManager.SelectOrderMapsetsBy.Value)
            {
                case OrderMapsetsBy.Difficulty:
                case OrderMapsetsBy.OnlineGrade:
                case OrderMapsetsBy.LongNotePercentage:
                case OrderMapsetsBy.NotesPerSecond:
                    return true;
                default:
                    return false;
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
                var set = new Mapset
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
        ///     Orders mapsets by their genre
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsByGenre(IEnumerable<Mapset> mapsets)
        {
            return mapsets.OrderBy(x => x.Maps.First().Genre).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders mapsets by length
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsByLength(IEnumerable<Mapset> mapsets)
        {
            return mapsets.OrderBy(x => x.Maps.First().SongLength).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders mapsets by notes per second
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsByNotesPerSecond(List<Mapset> mapsets, bool separateMapsets)
        {
            var newMapsets = new List<Mapset>();

            if (separateMapsets)
                newMapsets = SeparateMapsIntoOwnMapsets(mapsets);
            else
                newMapsets = mapsets;

            return newMapsets.OrderBy(x => x.Maps.First().NotesPerSecond).ToList();
        }

        /// <summary>
        ///     Orders mapsets by source
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsBySource(List<Mapset> mapsets)
        {
            return mapsets.OrderBy(x => x.Maps.First().Source).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders mapsets by the game they come from
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static List<Mapset> OrderMapsetsByGame(List<Mapset> mapsets)
        {
            return mapsets.OrderBy(x => x.Maps.First().Game).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();
        }

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
        ///     Orders mapsets by the amount of times the user played
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByTimesPlayed(IEnumerable<Mapset> mapsets)
        {
            return mapsets.OrderByDescending(x => x.Maps.Max(y => y.TimesPlayed))
                    .ThenBy(x => x.Maps.First().Artist)
                    .ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders mapsets by the most recently played
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsByRecentlyPlayed(List<Mapset> mapsets)
        {
            return mapsets.OrderByDescending(x => x.Maps.Max(y => y.LastTimePlayed))
                .ThenBy(x => x.Maps.First().Artist)
                .ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders the mapsets based on the set config value.
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="separateMapsets"></param>
        /// <param name="gameMode"></param>
        /// <param name="orderMapsetsBy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static List<Mapset> OrderMapsetsByConfigValue(List<Mapset> mapsets, bool separateMapsets = true,
            Bindable<SelectFilterGameMode> gameMode = null, Bindable<OrderMapsetsBy> orderMapsetsBy = null)
        {
            // Default to song select filter options
            if (gameMode == null)
                gameMode = ConfigManager.SelectFilterGameModeBy;

            if (orderMapsetsBy == null)
                orderMapsetsBy = ConfigManager.SelectOrderMapsetsBy;

            var mapsetsToRemove = new List<Mapset>();

            switch (gameMode?.Value)
            {
                case SelectFilterGameMode.All:
                    break;
                // Remove any maps that aren't 7K
                case SelectFilterGameMode.Keys4:
                    mapsets.ForEach(x =>
                    {
                        x.Maps.RemoveAll(y => y.Mode != GameMode.Keys4);

                        if (x.Maps.Count == 0)
                            mapsetsToRemove.Add(x);
                    });
                    break;
                // Remove any maps that aren't 7K
                case SelectFilterGameMode.Keys7:
                    mapsets.ForEach(x =>
                    {
                        x.Maps.RemoveAll(y => y.Mode != GameMode.Keys7);

                        if (x.Maps.Count == 0)
                            mapsetsToRemove.Add(x);
                    });
                    break;
            }

            mapsetsToRemove.ForEach(x => mapsets.Remove(x));

            switch (orderMapsetsBy?.Value)
            {
                case OrderMapsetsBy.Artist:
                    return OrderMapsetsByArtist(mapsets);
                case OrderMapsetsBy.Title:
                    return OrderMapsetsByTitle(mapsets);
                case OrderMapsetsBy.Creator:
                    return OrderMapsetsByCreator(mapsets);
                case OrderMapsetsBy.DateAdded:
                    return OrderMapsetsByDateAdded(mapsets);
                case OrderMapsetsBy.Status:
                    return OrderMapsetsByStatus(mapsets);
                case OrderMapsetsBy.BPM:
                    return OrderMapsetsByBpm(mapsets);
                case OrderMapsetsBy.TimesPlayed:
                    return OrderMapsetsByTimesPlayed(mapsets);
                case OrderMapsetsBy.RecentlyPlayed:
                    return OrderMapsetsByRecentlyPlayed(mapsets);
                case OrderMapsetsBy.Genre:
                    return OrderMapsetsByGenre(mapsets);
                case OrderMapsetsBy.Game:
                    return OrderMapsetsByGame(mapsets);
                case OrderMapsetsBy.Length:
                    return OrderMapsetsByLength(mapsets);
                case OrderMapsetsBy.Source:
                    return OrderMapsetsBySource(mapsets);
                case OrderMapsetsBy.Difficulty:
                    return OrderMapsetsByDifficulty(mapsets, separateMapsets);
                case OrderMapsetsBy.OnlineGrade:
                    return OrderMapsetsByOnlineGrade(mapsets, separateMapsets);
                case OrderMapsetsBy.DateLastUpdated:
                    return OrderMapsetsByDateLastUpdated(mapsets);
                case OrderMapsetsBy.DateRanked:
                    return OrderMapsetsByDateRanked(mapsets);
                case OrderMapsetsBy.LongNotePercentage:
                    return OrderMapsetsByLongNotePercentage(mapsets, separateMapsets);
                case OrderMapsetsBy.NotesPerSecond:
                    return OrderMapsetsByNotesPerSecond(mapsets, separateMapsets);
                default:
                    return mapsets.ToList();
            }
        }

        /// <summary>
        ///     Orders maps by difficulty.
        ///     In this, each map gets its own individual mapset
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="separateMapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsByDifficulty(List<Mapset> mapsets, bool separateMapsets)
        {
            var newMapsets = new List<Mapset>();

            if (separateMapsets)
                newMapsets = SeparateMapsIntoOwnMapsets(mapsets);
            else
                newMapsets = mapsets;

            return newMapsets.OrderBy(x => x.Maps.First().DifficultyFromMods(ModManager.Mods)).ToList();
        }

        /// <summary>
        ///     Highest online grade achieved
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="separateMapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByOnlineGrade(List<Mapset> mapsets, bool separateMapsets)
        {
            var newMapsets = new List<Mapset>();

            if (separateMapsets)
                newMapsets = SeparateMapsIntoOwnMapsets(mapsets);
            else
                newMapsets = mapsets;

            return newMapsets.OrderBy(x => GradeHelper.GetGradeImportanceIndex(x.Maps.First().OnlineGrade)).ToList();
        }

        /// <summary>
        ///     Sorts maps by their long note percentage
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="separateMapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByLongNotePercentage(List<Mapset> mapsets, bool separateMapsets)
        {
            var newMapsets = new List<Mapset>();

            if (separateMapsets)
                newMapsets = SeparateMapsIntoOwnMapsets(mapsets);
            else
                newMapsets = mapsets;

            return newMapsets.OrderBy(x => x.Maps.First().LNPercentage).ToList();
        }

        /// <summary>
        ///     Orders mapsets by the date they were last updated online
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsByDateLastUpdated(List<Mapset> mapsets)
        {
            return mapsets.OrderByDescending(x => x.Maps.Max(y => y.DateLastUpdated)).ThenBy(x => x.Maps.First().Artist)
                .ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders mapsets by the date they were ranked
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsByDateRanked(List<Mapset> mapsets)
        {
            return mapsets.OrderByDescending(x => x.Maps.First().RankedStatus)
                .ThenByDescending(x => x.Maps.Max(y => y.DateLastUpdated))
                .ThenBy(x => x.Maps.First().Artist)
                .ThenBy(x => x.Maps.First().Title).ToList();
        }

        /// <summary>
        ///     Orders the mapsets by BPM
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private static List<Mapset> OrderMapsetsByBpm(List<Mapset> mapsets)
            => mapsets.OrderBy(x => x.Maps.First().Bpm).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();

        /// <summary>
        ///     Orders the map's mapsets by date added
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByDateAdded(IEnumerable<Mapset> mapsets)
            => mapsets.OrderByDescending(x => x.Maps.First().DateAdded).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();

        /// <summary>
        ///     Orders mapsets by their ranked status
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> OrderMapsetsByStatus(IEnumerable<Mapset> mapsets)
            => mapsets.OrderByDescending(x => x.Maps.First().RankedStatus).ThenBy(x => x.Maps.First().Artist).ThenBy(x => x.Maps.First().Title).ToList();

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
        ///     Converts every map into a mapset of its own
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        internal static List<Mapset> SeparateMapsIntoOwnMapsets(List<Mapset> mapsets)
        {
            var newMapsets = new List<Mapset>();

            foreach (var mapset in mapsets)
            {
                foreach (var map in mapset.Maps)
                {
                    newMapsets.Add(new Mapset
                    {
                        Directory = mapset.Directory,
                        Maps = new List<Map> { map }
                    });
                }
            }

            return newMapsets;
        }

        /// <summary>
        ///     Separates a list of maps into their own mapsets
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        internal static List<Mapset> SeparateMapsIntoOwnMapsets(List<Map> maps)
        {
            var mapsets = new List<Mapset>();

            foreach (var map in maps)
            {
                var mapset = new Mapset
                {
                    Directory = map?.Mapset?.Directory ?? "",
                    Maps = new List<Map> {map}
                };

                mapsets.Add(mapset);
            }

            return mapsets;
        }

        /// <summary>
        /// Searches and returns mapsets given a query
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="query></param>
        /// <returns></returns>
        internal static List<Mapset> SearchMapsets(IEnumerable<Mapset> mapsets, string query)
        {
            var sets = new List<Mapset>();

            query = query.ToLower();

            // All the possible relational operators for the search query
            var operators = new List<string>
            {
                ">=",
                "<=",
                "==",
                "!=",
                "<",
                ">",
                "="
            };

            // The shortest and longest matching sequences for every option. For example, "d" and "difficulty" means you
            // can search for "d>5", "di>5", "dif>5" and so on up to "difficulty>5".
            //
            // When adding new options with overlapping first characters, specify the shortest unambiguous sequence as
            // the new key. So, for example, "duration" should be added as "du", "duration". This way "d" still searches
            // for "difficulty" (backwards-compatibility) and "du" searches for duration.
            var options = new Dictionary<SearchFilterOption, (string Shortest, string Longest)>
            {
                { SearchFilterOption.BPM,        ("b", "bpm") },
                { SearchFilterOption.Difficulty, ("d", "difficulty") },
                { SearchFilterOption.Length,     ("l", "length") },
                { SearchFilterOption.Keys,       ("k", "keys") },
                { SearchFilterOption.Status,     ("s", "status") },
                { SearchFilterOption.LNs,        ("ln", "lns") },
                { SearchFilterOption.NPS,        ("n", "nps") },
                { SearchFilterOption.Game,       ("g", "game") },
                { SearchFilterOption.TimesPlayed, ("t", "timesplayed") }
            };

            // Stores a dictionary of the found pairs in the search query
            // <option, value, operator>
            var foundSearchFilters = new List<SearchFilter>();

            var terms = query.Split(null).ToList();

            // Get a list of all the matching search filters.
            // All matched filters are removed from the list of terms.
            for (var i = terms.Count - 1; i >= 0; i--)
            {
                var term = terms[i];

                foreach (var op in operators)
                {
                    var match = Regex.Match(term, $@"(.+)\b{op}\b(.+)");
                    if (!match.Success)
                        continue;

                    var searchOption = match.Groups[1].Value;
                    var val = match.Groups[2].Value;

                    foreach (var (option, (shortest, longest)) in options)
                    {
                        if (longest.StartsWith(searchOption) && searchOption.StartsWith(shortest))
                        {
                            foundSearchFilters.Add(new SearchFilter
                            {
                                Option = option,
                                Value = val,
                                Operator = op
                            });

                            // Remove it from the search terms.
                            terms.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            var newMapsetLookup = new Dictionary<string, Mapset>();

            // Create a list of mapsets with the matched mapsets
            foreach (var mapset in mapsets)
            {
                foreach (var map in mapset.Maps)
                {
                    var exitLoop = false;

                    foreach (var searchQuery in foundSearchFilters)
                    {
                        switch (searchQuery.Option)
                        {
                            case SearchFilterOption.BPM:
                                if (!float.TryParse(searchQuery.Value, out var valBpm))
                                    exitLoop = true;

                                if (!CompareValues(map.Bpm, valBpm, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case SearchFilterOption.Difficulty:
                                if (!float.TryParse(searchQuery.Value, out var valDiff))
                                    exitLoop = true;

                                if (!CompareValues(map.DifficultyFromMods(ModManager.Mods), valDiff, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case SearchFilterOption.NPS:
                                if (!float.TryParse(searchQuery.Value, out var valNps))
                                    exitLoop = true;

                                var objectCount = map.LongNoteCount + map.RegularNoteCount;
                                var nps = (int) (objectCount / (map.SongLength / (1000 * ModHelper.GetRateFromMods(ModManager.Mods))));

                                if (!CompareValues(nps, valNps, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case SearchFilterOption.Length:
                                if (!float.TryParse(searchQuery.Value, out var valLength))
                                    exitLoop = true;

                                if (!CompareValues(map.SongLength / 1000f, valLength, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case SearchFilterOption.TimesPlayed:
                                if (!float.TryParse(searchQuery.Value, out var valTimesPlayed))
                                    exitLoop = true;

                                if (!CompareValues(map.TimesPlayed, valTimesPlayed, searchQuery.Operator))
                                    exitLoop = true;
                                break;
                            case SearchFilterOption.Keys:
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
                            case SearchFilterOption.Status:
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
                            case SearchFilterOption.Game:
                                if (!(searchQuery.Operator.Equals(operators[2]) ||
                                      searchQuery.Operator.Equals(operators[6])))
                                    exitLoop = true;

                                switch (map.Game)
                                {
                                    case MapGame.Quaver:
                                        if (!CompareValues("quaver", searchQuery.Value, searchQuery.Operator))
                                            exitLoop = true;
                                        break;
                                    case MapGame.Osu:
                                        if (!CompareValues("osu", searchQuery.Value, searchQuery.Operator))
                                            exitLoop = true;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                                break;
                            case SearchFilterOption.LNs:
                                var valueToCompareTo = 0;
                                var stringToParse = "";

                                if (searchQuery.Value.Last() == '%')
                                {
                                    stringToParse = searchQuery.Value.Substring(0, searchQuery.Value.Length - 1);
                                    valueToCompareTo = (int) map.LNPercentage;
                                }
                                else
                                {
                                    stringToParse = searchQuery.Value;
                                    valueToCompareTo = map.LongNoteCount;
                                }

                                if (!int.TryParse(stringToParse, out var value))
                                {
                                    exitLoop = true;
                                    break;
                                }

                                if (!CompareValues(valueToCompareTo, value, searchQuery.Operator))
                                    exitLoop = true;

                                break;
                        }

                        if (exitLoop)
                            break;
                    }

                    if (exitLoop)
                        continue;

                    // Check if the terms exist in any of the following properties.
                    foreach (var term in terms)
                    {
                        try
                        {
                            if (!map.Artist.ToLower().Contains(term) && !map.Title.ToLower().Contains(term) &&
                                !map.Creator.ToLower().Contains(term) && !map.Source.ToLower().Contains(term) &&
                                !map.Description.ToLower().Contains(term) && !map.Tags.ToLower().Contains(term) &&
                                !map.DifficultyName.ToLower().Contains(term) && !map.Genre.ToLower().Contains(term))
                            {
                                exitLoop = true;
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            // Some values can be null and they break the game.
                            exitLoop = true;
                            break;
                        }
                    }

                    if (exitLoop)
                        continue;

                    // Create a new mapset if it doesn't exist
                    if (!newMapsetLookup.ContainsKey(map.Directory))
                    {
                        var set = new Mapset()
                        {
                            Directory = map.Directory,
                            Maps = new List<Map>() {map}
                        };

                        sets.Add(set);
                        newMapsetLookup.Add(map.Directory, set);
                    }
                    // Otherwise just add the mapset to the existing set
                    else
                        newMapsetLookup[map.Directory].Maps.Add(map);
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
    ///     A struct for the map searching filter.
    /// </summary>
    public struct SearchFilter
    {
        /// <summary>
        ///     The search option - bpm, length, etc,
        /// </summary>
        public SearchFilterOption Option;

        /// <summary>
        ///     The value the user is searching
        /// </summary>
        public string Value;

        /// <summary>
        ///     The operator the user gave
        /// </summary>
        public string Operator;
    }

    /// <summary>
    ///     Options you can filter by.
    /// </summary>
    public enum SearchFilterOption
    {
        /// <summary>
        ///     BPM.
        /// </summary>
        BPM,

        /// <summary>
        ///     Difficulty rating.
        /// </summary>
        Difficulty,

        /// <summary>
        ///     Length in seconds.
        /// </summary>
        Length,

        /// <summary>
        ///     Key count.
        /// </summary>
        Keys,

        /// <summary>
        ///     Status (ranked, not submitted, etc.)
        /// </summary>
        Status,

        /// <summary>
        ///     LN count or percentage.
        /// </summary>
        LNs,

        /// <summary>
        ///     Notes Per Second
        /// </summary>
        NPS,

        /// <summary>
        ///     The game that the map comes from
        /// </summary>
        Game,

        /// <summary>
        ///     The amount of times the user has played the map
        /// </summary>
        TimesPlayed
    }
}
