/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Parsers;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Scores;
using SQLite;
using Wobble.Bindables;

namespace Quaver.Shared.Database.Maps
{
    public class Map
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        ///     The MD5 Of the file.
        /// </summary>
        [Unique]
        public string Md5Checksum { get; set; }

        /// <summary>
        ///     The directory of the map
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        ///     The absolute path of the .qua file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     The map set id of the map.
        /// </summary>
        public int MapSetId { get; set; }

        /// <summary>
        ///     The id of the map.
        /// </summary>
        public int MapId { get; set; }

        /// <summary>
        ///     The song's artist
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        ///     The song's title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The difficulty name of the map
        /// </summary>
        public string DifficultyName { get; set; }

        /// <summary>
        ///     The highest rank that the player has gotten on the map.
        /// </summary>
        public Grade HighestRank { get; set; }

        /// <summary>
        ///     The ranked status of the map.
        /// </summary>
        public RankedStatus RankedStatus { get; set; }

        /// <summary>
        ///     The last time the user has played the map.
        /// </summary>
        public string LastPlayed { get; set; } = new DateTime(0001, 1, 1, 00, 00, 00).ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        ///     The creator of the map.
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        ///     The absolute path of the map's background.
        /// </summary>
        public string BackgroundPath { get; set; }

        /// <summary>
        ///     The absolute path of the map's audio.
        /// </summary>
        public string AudioPath { get; set; }

        /// <summary>
        ///     The audio preview time of the map
        /// </summary>
        public int AudioPreviewTime { get; set; }

        /// <summary>
        ///     The description of the map
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     The source (album/mixtape/etc) of the map
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Tags for the map
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        ///     The most common bpm for the map
        /// </summary>
        public double Bpm { get; set; }

        /// <summary>
        ///     The map's length (Time of the last hit object)
        /// </summary>
        public int SongLength { get; set; }

        /// <summary>
        ///     The Game Mode of the map
        /// </summary>
        public GameMode Mode { get; set; }

        /// <summary>
        ///     The local offset for this map
        /// </summary>
        public int LocalOffset { get; set; }

        /// <summary>
        ///     The version of the difficulty calculator that was used in this cache
        /// </summary>
        public string DifficultyCalculatorVersion { get; set; }

        /// <summary>
        ///     The last time the file was modified
        /// </summary>
        public DateTime LastFileWrite { get; set; }

#region DIFFICULTY_RATINGS
        public double Difficulty05X { get; set; }
        public double Difficulty06X { get; set; }
        public double Difficulty07X { get; set; }
        public double Difficulty08X { get; set; }
        public double Difficulty09X { get; set; }
        public double Difficulty10X { get; set; }
        public double Difficulty11X { get; set; }
        public double Difficulty12X { get; set; }
        public double Difficulty13X { get; set; }
        public double Difficulty14X { get; set; }
        public double Difficulty15X { get; set; }
        public double Difficulty16X { get; set; }
        public double Difficulty17X { get; set; }
        public double Difficulty18X { get; set; }
        public double Difficulty19X { get; set; }
        public double Difficulty20X { get; set; }
 #endregion

        /// <summary>
        ///     Determines if this map is an osu! map.
        /// </summary>
        [Ignore]
        public MapGame Game { get; set; } = MapGame.Quaver;

        /// <summary>
        ///     The actual parsed qua file for the map.
        /// </summary>
        [Ignore]
        public Qua Qua { get; set; }

        /// <summary>
        ///     The mapset the map belongs to.
        /// </summary>
        [Ignore]
        public Mapset Mapset { get; set; }

        /// <summary>
        ///     The scores for this map.
        /// </summary>
        [Ignore]
        public Bindable<List<Score>> Scores { get; } = new Bindable<List<Score>>(null);

        /// <summary>
        ///     Determines if this map is outdated and needs an update.
        /// </summary>
        [Ignore]
        public bool NeedsOnlineUpdate { get; set; }

        /// <summary>
        ///     Responsible for converting a Qua object, to a Map object
        ///     a Map object is one that is stored in the db.
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Map FromQua(Qua qua, string path)
        {
            var map = new Map
            {
                Md5Checksum = MapsetHelper.GetMd5Checksum(path),
                Directory = new DirectoryInfo(System.IO.Path.GetDirectoryName(path) ?? throw new InvalidOperationException()).Name.Replace("\\", "/"),
                Path = System.IO.Path.GetFileName(path)?.Replace("\\", "/"),
                Artist = qua.Artist,
                Title = qua.Title,
                HighestRank = Grade.None,
                AudioPath = qua.AudioFile,
                AudioPreviewTime = qua.SongPreviewTime,
                BackgroundPath = qua.BackgroundFile,
                Description = qua.Description,
                MapId = qua.MapId,
                MapSetId = qua.MapSetId,
                Bpm = qua.GetCommonBpm(),
                Creator = qua.Creator,
                DifficultyName = qua.DifficultyName,
                Source = qua.Source,
                Tags = qua.Tags,
                SongLength =  qua.Length,
                Mode = qua.Mode,
            };

            map.LastFileWrite = File.GetLastWriteTimeUtc(map.Path);
            return map;
        }

        /// <summary>
        ///     Loads the .qua, .osu or .sm file for a map.
        ///
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Qua LoadQua()
        {
            // Reference to the parsed .qua file
            Qua qua;

            // Handle osu! maps as well
            switch (Game)
            {
                case MapGame.Quaver:
                    var quaPath = $"{ConfigManager.SongDirectory}/{Directory}/{Path}";
                    qua = Qua.Parse(quaPath);
                    break;
                case MapGame.Osu:
                    var osu = new OsuBeatmap(MapManager.OsuSongsFolder + Directory + "/" + Path);
                    qua = osu.ToQua();
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            return qua;
        }

        /// <summary>
        ///     Changes selected map
        /// </summary>
        public void ChangeSelected()
        {
            MapManager.Selected.Value = this;

            Task.Run(async () =>
            {
                using (var writer = File.CreateText(ConfigManager.DataDirectory + "/temp/Now Playing/map.txt"))
                {
                    await writer.WriteAsync($"{Artist} - {Title} [{DifficultyName}]");
                }
            });
        }

        /// <summary>
        ///     Unhooks all of the event handlers and clears the scores.
        /// </summary>
        public void ClearScores()
        {
            Scores.UnHookEventHandlers();
            Scores.Value?.Clear();
        }

        /// <summary>
        ///     Calculates the difficulty of the entire map
        /// </summary>
        public void CalculateDifficulties()
        {
            var qua = LoadQua();

            Difficulty05X = qua.SolveDifficulty(ModIdentifier.Speed05X).OverallDifficulty;
            Difficulty06X = qua.SolveDifficulty(ModIdentifier.Speed06X).OverallDifficulty;
            Difficulty07X = qua.SolveDifficulty(ModIdentifier.Speed07X).OverallDifficulty;
            Difficulty08X = qua.SolveDifficulty(ModIdentifier.Speed08X).OverallDifficulty;
            Difficulty09X = qua.SolveDifficulty(ModIdentifier.Speed09X).OverallDifficulty;
            Difficulty10X = qua.SolveDifficulty().OverallDifficulty;
            Difficulty11X = qua.SolveDifficulty(ModIdentifier.Speed11X).OverallDifficulty;
            Difficulty12X = qua.SolveDifficulty(ModIdentifier.Speed12X).OverallDifficulty;
            Difficulty13X = qua.SolveDifficulty(ModIdentifier.Speed13X).OverallDifficulty;
            Difficulty14X = qua.SolveDifficulty(ModIdentifier.Speed14X).OverallDifficulty;
            Difficulty15X = qua.SolveDifficulty(ModIdentifier.Speed15X).OverallDifficulty;
            Difficulty16X = qua.SolveDifficulty(ModIdentifier.Speed16X).OverallDifficulty;
            Difficulty17X = qua.SolveDifficulty(ModIdentifier.Speed17X).OverallDifficulty;
            Difficulty18X = qua.SolveDifficulty(ModIdentifier.Speed18X).OverallDifficulty;
            Difficulty19X = qua.SolveDifficulty(ModIdentifier.Speed19X).OverallDifficulty;
            Difficulty20X = qua.SolveDifficulty(ModIdentifier.Speed20X).OverallDifficulty;
        }

        /// <summary>
        ///     Retrieve the map's difficulty rating from given mods
        /// </summary>
        /// <returns></returns>
        public double DifficultyFromMods(ModIdentifier mods)
        {
            if (mods.HasFlag(ModIdentifier.Speed05X))
                return Difficulty05X;
            if (mods.HasFlag(ModIdentifier.Speed06X))
                return Difficulty06X;
            if (mods.HasFlag(ModIdentifier.Speed07X))
                return Difficulty07X;
            if (mods.HasFlag(ModIdentifier.Speed08X))
                return Difficulty08X;
            if (mods.HasFlag(ModIdentifier.Speed09X))
                return Difficulty09X;
            if (mods.HasFlag(ModIdentifier.Speed11X))
                return Difficulty11X;
            if (mods.HasFlag(ModIdentifier.Speed12X))
                return Difficulty12X;
            if (mods.HasFlag(ModIdentifier.Speed13X))
                return Difficulty13X;
            if (mods.HasFlag(ModIdentifier.Speed14X))
                return Difficulty14X;
            if (mods.HasFlag(ModIdentifier.Speed15X))
                return Difficulty15X;
            if (mods.HasFlag(ModIdentifier.Speed16X))
                return Difficulty16X;
            if (mods.HasFlag(ModIdentifier.Speed17X))
                return Difficulty17X;
            if (mods.HasFlag(ModIdentifier.Speed18X))
                return Difficulty18X;
            if (mods.HasFlag(ModIdentifier.Speed19X))
                return Difficulty19X;
            if (mods.HasFlag(ModIdentifier.Speed20X))
                return Difficulty20X;

            return Difficulty10X;
        }
    }

    /// <summary>
    ///     The game in which the map belongs to
    /// </summary>
    public enum MapGame
    {
        Quaver,
        Osu,
    }
}
