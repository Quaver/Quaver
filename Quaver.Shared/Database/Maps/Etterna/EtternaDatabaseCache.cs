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
                var additionalSongFoldersReplaceString = "";
                
                if (!File.Exists(preferencesFilePath))
                {
                    Logger.Warning($"Failed to load Etterna's additional songfolder information - Preferences.ini file does not exist at {preferencesFilePath}!", LogType.Runtime);
                }
                else{
                    // Open up the Preferences.ini file and retrieve the additional song folder.
                    var parser = new IniFileParser.IniFileParser();
                    IniData preferencesIniData = parser.ReadFile(preferencesFilePath);
                    additionalSongFoldersReplaceString = preferencesIniData["Options"]["AdditionalSongFolders"];
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

                    var map = new OtherGameMap()
                    {
                        Md5Checksum = step.ChartKey,
                        Directory = etternaDirectory + "/" + Path.GetDirectoryName(step.StepFileName),
                        Path = Regex.Replace(etternaDirectory + "/" + step.StepFileName, ".*AdditionalSongs", additionalSongFoldersReplaceString),
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

                    if (!File.Exists(map.Path))
                    {
                        Logger.Warning($"Skipping load on file: {step.StepFileName} because the file could not be found " +
                                       $"at: {map.Path}", LogType.Runtime);
                        continue;
                    }

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
