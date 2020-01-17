using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using SQLite;
using Wobble.Logging;

namespace Quaver.Shared.Database.Maps.Etterna
{
    public class EtternaDatabaseCache
    {
        public static List<OtherGameMap> Load()
        {
            if (string.IsNullOrEmpty(ConfigManager.EtternaDbPath.Value))
                return new List<OtherGameMap>();

            if (!File.Exists(ConfigManager.EtternaDbPath.Value))
            {
                Logger.Error($"Failed to load Ett database - cache.db file does not exist at that path!", LogType.Runtime);
                return new List<OtherGameMap>();
            }

            try
            {
                var connection = new SQLiteConnection(ConfigManager.EtternaDbPath.Value);

                var songs = connection.Table<EtternaSong>().ToDictionary(x => x.SongFileName);
                var steps = connection.Table<EtternaStep>().ToList();

                var maps = new List<OtherGameMap>();

                foreach (var step in steps)
                {
                    if (step.StepsType != "dance-single")
                        continue;

                    var song = songs[step.StepFileName];
                    var etternaDirectory = ConfigManager.EtternaDbPath.Value.Split("Etterna")[0] + "Etterna";

                    var map = new OtherGameMap()
                    {
                        Md5Checksum = step.ChartKey,
                        Directory = etternaDirectory + "/" + Path.GetDirectoryName(step.StepFileName),
                        Path = etternaDirectory + "/" + step.StepFileName,
                        MapSetId = -1,
                        MapId = -1,
                        Mode = GameMode.Keys4,
                        Game = MapGame.Etterna,
                        OriginalGame = OtherGameMapDatabaseGame.Etterna,
                        DifficultyName = step.Difficulty.ToString(),
                        Artist = song.Artist,
                        Title = song.Title,
                        Creator = step.Credit ?? song.Credit ?? "",
                        BackgroundPath = etternaDirectory + "/" + song.BackgroundPath,
                        BannerPath = etternaDirectory + "/" + song.BannerPath,
                        AudioPath = etternaDirectory + "/" + song.MusicPath,
                        AudioPreviewTime = (int) (song.SampleStart * 1000),
                        SongLength = (int) (song.MusicLength * 1000)
                    };

                    // Try and fetch the creator name from the directory name
                    // Example /Songs/Pack/Artist - Title (Creator)
                    if (string.IsNullOrEmpty(map.Creator))
                    {
                        var match = Regex.Match(map.Directory, @".* \((\w+\))");

                        if (match.Groups.Count != 0)
                            map.Creator = match.Groups[1].ToString().Replace(")", "");
                    }

                    map.Tags = new DirectoryInfo(Path.GetFullPath(Path.Combine(map.Directory, @"..\"))).Name;
                    maps.Add(map);
                }

                connection.Close();
                return maps;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load Ett database!", LogType.Runtime);
                Logger.Error(e, LogType.Runtime);
            }

            return new List<OtherGameMap>();
        }
    }
}