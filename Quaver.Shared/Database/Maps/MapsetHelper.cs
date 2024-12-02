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
using WilliamQiufeng.SearchParser.AST;
using WilliamQiufeng.SearchParser.Parsing;
using WilliamQiufeng.SearchParser.Tokenizing;
using Wobble.Bindables;

namespace Quaver.Shared.Database.Maps
{
    public static class MapsetHelper
    {
        /// <summary>
        ///     A trie containing all possible keywords (keys like lns, mode, and enum values like unranked, quaver)
        /// </summary>
        private static Trie _searchKeywordTrie = new();

        /// <summary>
        ///     Stores which <see cref="SearchFilterOption"/> an enum value belongs to.
        ///     For example, pair (<see cref="MapGame.Quaver"/>, <see cref="SearchFilterOption.Game"/>) exists in the
        ///     dictionary.
        /// </summary>
        private static readonly Dictionary<object, SearchFilterOption> SearchEnumKeyDictionary = new();

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
            var gameModes = musicPlayer ? new Bindable<SelectFilterGameMode>(SelectFilterGameMode.All) { Value = SelectFilterGameMode.All } : null;
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
                    Maps = new List<Map> { map }
                };

                mapsets.Add(mapset);
            }

            return mapsets;
        }

        static MapsetHelper()
        {
            InitializeSearchKeywordTrie();
        }

        internal static void InitializeSearchKeywordTrie()
        {
            _searchKeywordTrie = new Trie();
            var keys = new Dictionary<string, SearchFilterOption>
            {
                 ["artists"] = SearchFilterOption.Artist,
                 ["bpm"] = SearchFilterOption.BPM,
                 ["creators"] = SearchFilterOption.Creator,
                 ["difficulty"] = SearchFilterOption.Difficulty,
                 ["description"] = SearchFilterOption.Description,
                 ["difficultyname"] = SearchFilterOption.DifficultyName,
                 ["diffname"] = SearchFilterOption.DifficultyName,
                 ["game"] = SearchFilterOption.Game,
                 ["genre"] = SearchFilterOption.Genre,
                 ["keys"] = SearchFilterOption.Keys,
                 ["length"] = SearchFilterOption.Length,
                 ["lns"] = SearchFilterOption.LNs,
                 ["nps"] = SearchFilterOption.NPS,
                 ["title"] = SearchFilterOption.Title,
                 ["tags"] = SearchFilterOption.Tags,
                 ["timesplayed"] = SearchFilterOption.TimesPlayed,
                 ["status"] = SearchFilterOption.Status,
                 ["sources"] = SearchFilterOption.Source
            };

            var gameTypes = new Dictionary<string, MapGame>
            {
                ["quaver"] = MapGame.Quaver,
                ["osu"] = MapGame.Osu,
                ["etterna"] = MapGame.Etterna
            };

            var rankedStatuses = new Dictionary<string, RankedStatus>
            {
                ["ranked"] = RankedStatus.Ranked,
                ["unranked"] = RankedStatus.Unranked,
                ["notsubmitted"] = RankedStatus.NotSubmitted,
                ["unsubmitted"] = RankedStatus.NotSubmitted,
            };

            SearchEnumKeyDictionary.Clear();


            foreach (var (key, option) in keys)
            {
                _searchKeywordTrie.Add(key, TokenKind.Key, option);
            }

            foreach (var (name, mapGame) in gameTypes)
            {
                _searchKeywordTrie.Add(name, TokenKind.Enum, mapGame);
                // TryAdd since we have multiple possible enum strings that point to the same value
                SearchEnumKeyDictionary.TryAdd(mapGame, SearchFilterOption.Game);
            }

            foreach (var (name, rankedStatus) in rankedStatuses)
            {
                _searchKeywordTrie.Add(name, TokenKind.Enum, rankedStatus);
                // TryAdd since we have multiple possible enum strings that point to the same value
                SearchEnumKeyDictionary.TryAdd(rankedStatus, SearchFilterOption.Status);
            }
        }

        /// <summary>
        /// Searches and returns mapsets given a query
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        internal static List<Mapset> SearchMapsets(IEnumerable<Mapset> mapsets, string query)
        {
            var sets = new List<Mapset>();

            query = query.ToLower();

            var tokenizer = new Tokenizer(query);
            tokenizer.KeywordTrie = _searchKeywordTrie;

            var parser = new Parser(tokenizer);
            parser.SearchCriterionConstraint = criterion =>
            {
                // This is redundant check, but just in case...
                if (criterion.Values.Count == 0)
                    return false;

                var option = (SearchFilterOption)criterion.Key.Value!;
                var valueKind = criterion.Values.FirstOrDefault()?.Token.Kind;

                if (valueKind is TokenKind.Enum)
                    return criterion.Values.All(v =>
                        SearchEnumKeyDictionary.TryGetValue(v.Value!, out var key) && key.Equals(criterion.Key.Value));

                return option switch
                {
                    SearchFilterOption.BPM or SearchFilterOption.Difficulty when criterion.Values.Count == 1 =>
                        valueKind is TokenKind.Integer or TokenKind.Real,
                    SearchFilterOption.Length when criterion.Values.Count == 1 =>
                        valueKind is TokenKind.Integer or TokenKind.TimeSpan,
                    SearchFilterOption.Status or SearchFilterOption.Game =>
                        valueKind is TokenKind.Enum,
                    SearchFilterOption.Keys when criterion.Values.Count == 1 =>
                        valueKind is TokenKind.Integer,
                    SearchFilterOption.LNs when criterion.Values.Count == 1 =>
                        valueKind is TokenKind.Integer or TokenKind.Percentage,
                    SearchFilterOption.NPS or SearchFilterOption.TimesPlayed when criterion.Values.Count == 1 =>
                        valueKind is TokenKind.Integer,
                    SearchFilterOption.Title or SearchFilterOption.Tags or
                        SearchFilterOption.Source or SearchFilterOption.Artist or
                        SearchFilterOption.Creator or SearchFilterOption.Description or
                        SearchFilterOption.DifficultyName or SearchFilterOption.Genre =>
                        valueKind is TokenKind.String or TokenKind.PlainText,
                    _ => false
                };
            };

            parser.SingletonEnumProcessor = listValue =>
            {
                // List must not be empty
                if (listValue.Count == 0) return Array.Empty<SearchCriterion>();
                // First value must have valid key correspondence
                if (!SearchEnumKeyDictionary.TryGetValue(listValue[0].Value!, out var firstValueKey))
                    return Array.Empty<SearchCriterion>();
                // Coherent types for all values
                if (listValue.Any(v =>
                        !SearchEnumKeyDictionary.TryGetValue(v.Value!, out var filterOption) ||
                        filterOption != firstValueKey))
                    return Array.Empty<SearchCriterion>();
                return new[] { new SearchCriterion(firstValueKey, TokenKind.Equal, listValue, false) };
            };

            parser.Parse();

            var ast = parser.GetAst();

            var newMapsetLookup = new Dictionary<string, Mapset>();

            // Create a list of mapsets with the matched mapsets
            foreach (var mapset in mapsets)
            {
                foreach (var map in mapset.Maps)
                {
                    if (!ast.Evaluate(map, EvaluateMapAgainstQuery))
                        continue;

                    // Create a new mapset if it doesn't exist
                    if (!newMapsetLookup.ContainsKey(map.Directory))
                    {
                        var set = new Mapset()
                        {
                            Directory = map.Directory,
                            Maps = new List<Map>()
                        };
                        set.Maps.Add(map);

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
        ///     Checks if the map matches the given query.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static bool EvaluateMapAgainstQuery(AtomCriterionAst searchQuery, Map map)
        {
            var operatorKind = searchQuery.Operator.Kind;
            var valString = "";
            if (searchQuery.Key.Kind is TokenKind.PlainText)
            {
                var termContent = (string)searchQuery.Value.Value!;
                try
                {
                    var containsText = !map.Artist.ToLower().Contains(termContent) &&
                                       !map.Title.ToLower().Contains(termContent) &&
                                       !map.Creator.ToLower().Contains(termContent) &&
                                       !map.Source.ToLower().Contains(termContent) &&
                                       !map.Description.ToLower().Contains(termContent) &&
                                       !map.Tags.ToLower().Contains(termContent) &&
                                       !map.DifficultyName.ToLower().Contains(termContent) &&
                                       !map.Genre.ToLower().Contains(termContent);
                    if (containsText)
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    // Some values can be null and they break the game.
                    return false;
                }

                return true;
            }
            switch ((SearchFilterOption)searchQuery.Key.Value!)
            {
                case SearchFilterOption.BPM:
                    var valBpm = searchQuery.Value.Value switch
                    {
                        int i => i,
                        double d => d,
                        _ => 0
                    };
                    return CompareValues(map.Bpm, valBpm, operatorKind, false);
                case SearchFilterOption.Difficulty:
                    var valDiff = searchQuery.Value.Value switch
                    {
                        int i => i,
                        double d => d,
                        _ => 0
                    };

                    return CompareValues(map.DifficultyFromMods(ModManager.Mods), valDiff, operatorKind, false);
                case SearchFilterOption.NPS:
                    var valNps = (double)searchQuery.Value.Value!;
                    var objectCount = map.LongNoteCount + map.RegularNoteCount;
                    var nps = (objectCount /
                               (map.SongLength / (1000 * ModHelper.GetRateFromMods(ModManager.Mods))));

                    return CompareValues(nps, valNps, operatorKind, false);
                case SearchFilterOption.Length:
                    var valLength = searchQuery.Value.Value switch
                    {
                        TimeSpan timeSpan => timeSpan,
                        int seconds => TimeSpan.FromSeconds(seconds),
                        _ => throw new InvalidOperationException()
                    };
                    return CompareValues(map.SongLength / 1000f, valLength.TotalSeconds, operatorKind, false);
                case SearchFilterOption.TimesPlayed:
                    var valTimesPlayed = (int)searchQuery.Value.Value!;
                    return CompareValues(map.TimesPlayed, valTimesPlayed, operatorKind, false);
                case SearchFilterOption.Keys:
                    var valKeys = (int)searchQuery.Value.Value!;
                    switch (map.Mode)
                    {
                        case GameMode.Keys4:
                            var keyCount = map.HasScratchKey ? 5 : 4;
                            return CompareValues(keyCount, valKeys, operatorKind, false);
                        case GameMode.Keys7:
                            var keyCount7k = map.HasScratchKey ? 8 : 7;
                            return CompareValues(keyCount7k, valKeys, operatorKind, false);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case SearchFilterOption.Status:
                    return CompareEquality(map.RankedStatus,
                        searchQuery.Value.Value,
                        searchQuery.Operator.Kind, false);
                case SearchFilterOption.Game:
                    return CompareEquality(map.Game,
                            searchQuery.Value.Value,
                            searchQuery.Operator.Kind, false);
                case SearchFilterOption.LNs:
                    var valueToCompareTo = searchQuery.Value.Token.Kind is TokenKind.Percentage
                        ? (int)map.LNPercentage
                        : map.LongNoteCount;
                    var valLns = (int)searchQuery.Value.Value!;
                    return CompareValues(valueToCompareTo, valLns, operatorKind, false);
                case SearchFilterOption.Title:
                    valString = map.Title;
                    break;
                case SearchFilterOption.Tags:
                    valString = map.Tags;
                    break;
                case SearchFilterOption.Source:
                    valString = map.Source;
                    break;
                case SearchFilterOption.Artist:
                    valString = map.Artist;
                    break;
                case SearchFilterOption.Creator:
                    valString = map.Creator;
                    break;
                case SearchFilterOption.Description:
                    valString = map.Description;
                    break;
                case SearchFilterOption.DifficultyName:
                    valString = map.DifficultyName;
                    break;
                case SearchFilterOption.Genre:
                    valString = map.Genre;
                    break;
            }

            if (string.IsNullOrEmpty(valString))
                return false;

            return CompareValues(valString.ToLower(),
                searchQuery.Value.Value?.ToString()?.ToLower(),
                operatorKind, false);

        }

        /// <summary>
        ///     Compares two values and determines
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <param name="operation"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
        private static bool CompareValues<T>(T val1, T val2, TokenKind operation, bool invert) where T : IComparable<T>
        {
            if (val1 == null || val2 == null)
                return false;

            var compared = val1.CompareTo(val2);

            var result = operation switch
            {
                TokenKind.LessThan => compared < 0,
                TokenKind.MoreThan => compared > 0,
                TokenKind.Equal => compared == 0,
                TokenKind.LessThanOrEqual => compared <= 0,
                TokenKind.MoreThanOrEqual => compared >= 0,
                TokenKind.NotEqual => compared != 0,
                TokenKind.Contains when val1 is string s1 && val2 is string s2 => s1.Contains(s2),
                _ => false
            };
            return result ^ invert;
        }

        /// <summary>
        ///     Compares two values and determines
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <param name="operation"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
        private static bool CompareEquality<T>(T val1, T val2, TokenKind operation, bool invert)
        {
            if (val1 == null || val2 == null)
                return false;

            var result = operation switch
            {
                TokenKind.Contains when val1 is string s1 && val2 is string s2 => s1.Contains(s2),
                TokenKind.Equal when val1 is string s1 && val2 is string s2 => s1.Contains(s2),
                TokenKind.NotEqual when val1 is string s1 && val2 is string s2 => !s1.Contains(s2),
                TokenKind.Equal => Equals(val1, val2),
                TokenKind.NotEqual => !Equals(val1, val2),
                _ => false
            };
            return result ^ invert;
        }

        /// <summary>
        ///     Compares a value once for each value in a given list and returns according to a given logic operator
        /// </summary>
        /// <param name="val1">The initial value to compare to the others</param>
        /// <param name="values">The values to compare to</param>
        /// <param name="operation">The operation used to compare</param>
        /// <param name="mode">Logic gate used to return, either "and" or "or</param>
        /// <param name="invert"></param>
        private static bool CompareToMultipleValues<T>(T val1, T[] values, TokenKind operation,
            ListCombinationKind mode, bool invert)
        {
            var result = mode switch
            {
                ListCombinationKind.And =>
                    values.All(valToCompare => CompareEquality(val1, valToCompare, operation, false)),
                ListCombinationKind.Or or ListCombinationKind.None =>
                    values.Any(valToCompare => CompareEquality(val1, valToCompare, operation, false)),
                _ => false
            };
            return result ^ invert;
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
        TimesPlayed,

        /// <summary>
        ///     The title of the map
        /// </summary>
        Title,

        /// <summary>
        ///     The tags of the map
        /// </summary>
        Tags,

        /// <summary>
        ///     The source of the map
        /// </summary>
        Source,

        /// <summary>
        ///     The artist of the map
        /// </summary>
        Artist,

        /// <summary>
        ///     The creator of the map
        /// </summary>
        Creator,

        /// <summary>
        ///     The description of the map
        /// </summary>
        Description,

        /// <summary>
        ///     The difficulty name of the map
        /// </summary>
        DifficultyName,

        /// <summary>
        ///     The genre of the map
        /// </summary>
        Genre
    }
}
