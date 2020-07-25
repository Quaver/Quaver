using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using SQLite;
using Wobble.Logging;
using IniFileParser;
using IniFileParser.Model;

namespace Quaver.Shared.Database.Maps.Etterna
{
    public class EtternaDatabaseCache
    {
        private static readonly string cacheDatabaseRegularExpression = @"[\\/]Cache[\\/]cache\.db$";

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

                var songs = connection.Table<EtternaSong>();
                var steps = connection.Table<EtternaStep>().ToList();
                var songDictionary = new Dictionary<string, EtternaSong>();

                foreach (var song in songs)
                {
                    if (!songDictionary.ContainsKey(song.SongFileName))
                        songDictionary.Add(song.SongFileName, song);
                }

                var maps = new List<OtherGameMap>();

                // Trim the cache.db off the end using a regular expression.
                // The path to the DB file should be constant.
                var etternaDirectory = Regex.Replace(ConfigManager.EtternaDbPath.Value, cacheDatabaseRegularExpression, "");

                // Store the location of Preferences.ini
                var preferencesFilePath = etternaDirectory + @"/Save/Preferences.ini";

                // Etterna handles additional song folders by replacing "AdditionalSongs/" by a custom set song folder.
                // That is replicated here by using a regex.
                string[] additionalSongFolders = {};

                if (!File.Exists(preferencesFilePath))
                {
                    Logger.Warning($"Failed to load Etterna's additional songfolder information - Preferences.ini file " +
                                   $"does not exist at {preferencesFilePath}!", LogType.Runtime);
                }
                else
                {
                    // Open up the Preferences.ini file and retrieve the additional song folder.
                    var parser = new IniFileParser.IniFileParser();
                    var preferencesIniData = parser.ReadFile(preferencesFilePath);
                    additionalSongFolders = preferencesIniData["Options"]["AdditionalSongFolders"].Split(',');
                }

                foreach (var step in steps)
                {
                    if (step.StepsType != "dance-single")
                        continue;

                    // Not reading .ssc for now
                    if (step.StepFileName.EndsWith(".ssc"))
                        continue;

                    if (!songDictionary.ContainsKey(step.StepFileName))
                        continue;

                    var song = songDictionary[step.StepFileName];

                    var mapPath = etternaDirectory + "/" + step.StepFileName;

                    foreach (var possibleSongfolder in additionalSongFolders)
                    {
                        var possiblePath = Regex.Replace(etternaDirectory + "/" + step.StepFileName, ".*AdditionalSongs", possibleSongfolder);
                        if (File.Exists(possiblePath))
                        {
                            mapPath = possiblePath;
                            break;
                        }
                    }

                    if (!File.Exists(mapPath))
                    {
                        Logger.Warning($"Skipping load on file: {step.StepFileName} because the file could not be found at: {mapPath}", LogType.Runtime);
                        continue;
                    }

                    var directory = Path.GetDirectoryName(mapPath);

                    var map = new OtherGameMap()
                    {
                        Md5Checksum = step.ChartKey,
                        Directory = directory,
                        Path = mapPath,
                        MapSetId = -1,
                        MapId = -1,
                        Mode = GameMode.Keys4,
                        Game = MapGame.Etterna,
                        OriginalGame = OtherGameMapDatabaseGame.Etterna,
                        DifficultyName = step.Difficulty.ToString(),
                        Artist = song.Artist,
                        Title = song.Title,
                        Creator = step.Credit ?? song.Credit ?? "",
                        BackgroundPath = directory + "/" + Path.GetFileName(song.BackgroundPath),
                        BannerPath = directory + "/" + Path.GetFileName(song.BannerPath),
                        AudioPath = directory + "/" + Path.GetFileName(song.MusicPath),
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

                    map.Tags = new DirectoryInfo(Path.GetFullPath(Path.Combine(map.Directory, @"../"))).Name;
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
