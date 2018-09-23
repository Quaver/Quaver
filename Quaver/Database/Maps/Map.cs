using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Parsers;
using Quaver.API.Qss;
using Quaver.Config;
using Quaver.Database.Scores;
using SQLite;
using Wobble;
using Wobble.Bindables;

namespace Quaver.Database.Maps
{
    public class Map
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        ///     The MD5 Of the file.
        /// </summary>
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
        ///     The difficulty rating of the map.
        /// </summary>
        public float DifficultyRating { get; set; }

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
        ///     Computed Data relating to the map's difficulty
        /// </summary>
        [Ignore]
        public StrainSolver StrainSolver { get; set; }

        /// <summary>
        ///     The mapset the map belongs to.
        /// </summary>
        [Ignore]
        public Mapset Mapset { get; set; }

        /// <summary>
        ///     The scores for this map.
        /// </summary>
        [Ignore]
        public Bindable<List<LocalScore>> Scores { get; } = new Bindable<List<LocalScore>>(null);

        /// <summary>
        ///     Responsible for converting a Qua object, to a Map object
        ///     a Map object is one that is stored in the db.
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Map FromQua(Qua qua, string path) => new Map
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
            DifficultyRating = qua.AverageNotesPerSecond()
        };

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
                    StrainSolver = new StrainSolver(qua);
                    break;
                case MapGame.Osu:
                    var osu = new OsuBeatmap(MapManager.OsuSongsFolder + Directory + "/" + Path);
                    qua = osu.ToQua();
                    StrainSolver = new StrainSolver(qua);
                    break;
                case MapGame.Etterna:
                    // In short, find the chart with the same DifficultyName. There's literally no other way for us to check
                    // other than through this means.
                    var sm = StepManiaFile.Parse(MapManager.EtternaFolder + Directory + "/" + Path).ToQua();
                    qua = sm.Find(x => x.DifficultyName == DifficultyName);
                    StrainSolver = new StrainSolver(qua);
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
    }

    /// <summary>
    ///     The game in which the map belongs to
    /// </summary>
    public enum MapGame
    {
        Quaver,
        Osu,
        Etterna
    }
}