using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Parsers;
using Quaver.API.Maps.Parsers.Stepmania;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Playlists;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SQLite;
using Wobble;
using Wobble.Logging;
using Wobble.Platform;

namespace Quaver.Shared.Database.Playlists
{
    public class Playlist
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        ///     The name of the playlist
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The person who created the playlist
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        ///     Small description about the playlist
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     The kind of playlist this is.
        /// </summary>
        public PlaylistType Type { get; set; }

        /// <summary>
        ///     The ID for the map pool of the playlist (if exists)
        /// </summary>
        public int OnlineMapPoolId { get; set; } = -1;

        /// <summary>
        ///     The id of the creator of the map pool
        /// </summary>
        public int OnlineMapPoolCreatorId { get; set; } = -1;

        /// <summary>
        ///     The maps that are inside of the playlist
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public List<Map> Maps { get; set; } = new List<Map>();

        /// <summary>
        ///     The modifiers saved for each map in a tournament playlist, keyed by map MD5.
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public Dictionary<string, long> MapModifiers { get; set; } = new Dictionary<string, long>();

        /// <summary>
        ///     Fully processed tournament difficulty ratings, keyed by map MD5 and invalidated when its mods change.
        /// </summary>
        [Ignore]
        [JsonIgnore]
        private ConcurrentDictionary<string, KeyValuePair<long, double>> MapDifficulties { get; } =
            new ConcurrentDictionary<string, KeyValuePair<long, double>>();

        /// <summary>
        ///     Mods that alter the chart data consumed by the difficulty processor.
        /// </summary>
        private const ModIdentifier DifficultyTransformingModifiers = ModIdentifier.NoMines |
                                                                      ModIdentifier.NoLongNotes |
                                                                      ModIdentifier.Inverse |
                                                                      ModIdentifier.FullLN |
                                                                      ModIdentifier.Mirror;

        /// <summary>
        ///     The game the playlist is from
        /// </summary>
        [Ignore]
        public MapGame PlaylistGame { get; set; }

        /// <summary>
        ///     Returns if the playlist is an online playlist
        /// </summary>
        /// <returns></returns>
        public bool IsOnlineMapPool() => OnlineMapPoolId != -1;

        /// <summary>
        ///     Returns if this playlist supports per-map tournament modifiers.
        /// </summary>
        public bool IsTournament() => Type == PlaylistType.Tournament;

        /// <summary>
        ///     Returns if the current user owns this playlist and can edit its tournament settings.
        ///     Local Quaver playlists are owned locally; online map pools use their server owner ID.
        /// </summary>
        public bool IsOwnedByCurrentUser()
        {
            if (PlaylistGame != MapGame.Quaver)
                return false;

            if (OnlineMapPoolCreatorId != -1)
                return OnlineMapPoolCreatorId == OnlineManager.Self?.OnlineUser.Id;

            return string.Equals(Creator?.Trim(), ConfigManager.Username?.Value?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Gets the saved modifiers for a map in this playlist.
        /// </summary>
        public long GetMapModifiers(Map map)
        {
            if (map?.Md5Checksum == null)
                return 0;

            return MapModifiers.TryGetValue(map.Md5Checksum, out var modifiers) ? modifiers : 0;
        }

        /// <summary>
        ///     Gets the difficulty rating after applying the map's persisted tournament modifiers.
        /// </summary>
        public double GetMapDifficulty(Map map)
        {
            if (map?.Md5Checksum == null)
                return 0;

            var modifiers = GetMapModifiers(map);

            if (MapDifficulties.TryGetValue(map.Md5Checksum, out var cached) && cached.Key == modifiers)
                return cached.Value;

            var identifier = (ModIdentifier)modifiers;
            double difficulty;

            try
            {
                difficulty = (identifier & DifficultyTransformingModifiers) == 0
                    ? map.DifficultyFromMods(identifier)
                    : map.LoadQua(false).SolveDifficulty(identifier, true).OverallDifficulty;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to process tournament difficulty for {map}", LogType.Runtime);
                Logger.Error(e, LogType.Runtime);
                difficulty = map.DifficultyFromMods(identifier);
            }

            MapDifficulties[map.Md5Checksum] = new KeyValuePair<long, double>(modifiers, difficulty);
            return difficulty;
        }

        /// <summary>
        ///     Processes and caches every map difficulty before tournament rows are displayed.
        /// </summary>
        public void PrepareMapDifficulties()
        {
            if (!IsTournament())
                return;

            foreach (var map in Maps)
                GetMapDifficulty(map);
        }

        /// <summary>
        ///     Invalidates a map's processed tournament difficulty after its modifiers change.
        /// </summary>
        public void InvalidateMapDifficulty(Map map)
        {
            if (map?.Md5Checksum != null)
                MapDifficulties.TryRemove(map.Md5Checksum, out _);
        }

        /// <summary>
        ///     Exports the playlist to a directory
        /// </summary>
        public void Export(ExportMode exportMode)
        {
            var mapsetsAdded = new List<Mapset>();
            var mapsMd5 = new List<string>();
            var mapModifiers = new List<PlaylistMapExportMetadata>();

            var tempFolder = $"{ConfigManager.TempDirectory}/{GameBase.Game.TimeRunning}/";
            Directory.CreateDirectory(tempFolder);
            var exportsDirectory = $"{ConfigManager.DataDirectory}/Exports";
            Directory.CreateDirectory(exportsDirectory);
            
            var outputPath = $"{exportsDirectory}/{StringHelper.FileNameSafeString(Name)}.{(exportMode == ExportMode.Zip ? "zip" : "qpl")}";

            using (var archive = ZipArchive.Create())
            {
                foreach (var map in Maps)
                {
                    var exportedMd5 = map.GetAlternativeMd5();
                    mapsMd5.Add(exportedMd5);

                    if (IsTournament())
                    {
                        var modifiers = (ModIdentifier)GetMapModifiers(map);
                        mapModifiers.Add(new PlaylistMapExportMetadata
                        {
                            Md5 = exportedMd5,
                            Modifiers = (long)modifiers,
                            Rate = ModHelper.GetRateFromMods(modifiers)
                        });
                    }

                    if (mapsetsAdded.Contains(map.Mapset))
                        continue;

                    try
                    {
                        var exportDirectory = map.Game == MapGame.Etterna
                            ? StringHelper.FileNameSafeString($"{map.Artist} - {map.Title} [{map.DifficultyName}]")
                            : map.Directory;

                        Directory.CreateDirectory($"{tempFolder}/{exportDirectory}");

                        switch (map.Game)
                        {
                            // Quaver mapset, so just copy over the folder
                            case MapGame.Quaver:
                                foreach (var file in Directory.GetFiles($"{ConfigManager.SongDirectory.Value}/{map.Directory}"))
                                    File.Copy(file, $"{tempFolder}/{exportDirectory}/{Path.GetFileName(file)}");
                                break;
                            // osu! mapset - .qua files need to be
                            case MapGame.Osu:
                                var osuPath = $"{MapManager.OsuSongsFolder}{map.Directory}/{map.Path}";

                                var osu = new OsuBeatmap(osuPath);
                                map.BackgroundPath = osu.Background;

                                var name = StringHelper.FileNameSafeString($"{map.Artist} - {map.Title} [{map.DifficultyName}].qua");
                                var savePath = $"{tempFolder}/{exportDirectory}/{name}";

                                osu.ToQua().Save(savePath);

                                foreach (var file in Directory.GetFiles($"{MapManager.OsuSongsFolder}{map.Directory}/"))
                                {
                                    if (!file.EndsWith(".osu") && !file.EndsWith(".osb"))
                                        File.Copy(file, $"{tempFolder}/{exportDirectory}/{Path.GetFileName(file)}");
                                }
                                break;
                            case MapGame.Etterna:
                                var folderPath = Path.GetDirectoryName(map.Path);
                                var stepFile = new StepFile(map.Path);

                                var fileName = StringHelper.FileNameSafeString($"{map.Artist} - {map.Title} [{map.DifficultyName}].qua");
                                var fileSavePath = $"{tempFolder}/{exportDirectory}/{fileName}";
                                stepFile.ToQuas().Find(x => x.DifficultyName == map.DifficultyName).Save(fileSavePath);

                                foreach (var file in Directory.GetFiles(folderPath))
                                {
                                    if (!file.EndsWith(".sm") && !file.EndsWith(".ssc"))
                                        File.Copy(file, $"{tempFolder}/{exportDirectory}/{Path.GetFileName(file)}");
                                }
                                break;
                        }

                        using (var mapArchive = ZipArchive.Create())
                        {
                            mapArchive.AddAllFromDirectory($"{tempFolder}/{exportDirectory}/");
                            mapArchive.SaveTo($"{tempFolder}/{exportDirectory}.qp", CompressionType.Deflate);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                    }
                    finally
                    {
                        mapsetsAdded.Add(map.Mapset);
                    }
                }

                // add playlist metadata
                if (exportMode == ExportMode.Playlist)
                {
                    var metadata = new PlaylistExportMetadata
                    {
                        Name = Name,
                        Creator = Creator,
                        Description = Description,
                        Type = Type,
                        OnlineMapPoolId = OnlineMapPoolId,
                        OnlineMapPoolCreatorId = OnlineMapPoolCreatorId,
                        Maps = mapsMd5,
                        MapModifiers = mapModifiers
                    };

                    var json = JsonConvert.SerializeObject(metadata);
                    File.WriteAllText($"{tempFolder}/metadata.json", json);
                    archive.AddAllFromDirectory(tempFolder, "*.json");
                }
                
                archive.AddAllFromDirectory(tempFolder, "*.qp");
                archive.SaveTo(outputPath, CompressionType.Deflate);
            }

            Directory.Delete(tempFolder, true);
            Logger.Important($"Playlist `{Name} (#{Id}) has been successfully exported`", LogType.Runtime);

            Utils.NativeUtils.HighlightInFileManager(outputPath);
        }

        /// <summary>
        ///     Returns any maps that are missing in the pool
        /// </summary>
        /// <returns></returns>
        public List<int> GetMissingMapPoolMaps()
        {
            if (!IsOnlineMapPool())
                return new List<int>();

            var missing = new List<int>();

            var response = new APIRequestPlaylistMaps(this).ExecuteRequest();

            foreach (var id in response.MapIds)
            {
                var map = MapManager.FindMapFromOnlineId(id);

                // Map is already in playlist or doesn't exist
                if (map == null)
                    missing.Add(id);
            }

            return missing;
        }

        public void OpenUrl() => BrowserHelper.OpenURL($"https://quavergame.com/playlist/{OnlineMapPoolId}");

        public enum ExportMode
        {
            Zip,
            Playlist
        }

        internal class PlaylistExportMetadata
        {
            public string Name { get; set; }

            public string Creator { get; set; }

            public string Description { get; set; }

            public PlaylistType Type { get; set; }

            public int OnlineMapPoolId { get; set; }

            public int OnlineMapPoolCreatorId { get; set; }

            public List<string> Maps { get; set; }

            public List<PlaylistMapExportMetadata> MapModifiers { get; set; } = new List<PlaylistMapExportMetadata>();
        }

        internal class PlaylistMapExportMetadata
        {
            public string Md5 { get; set; }

            public long Modifiers { get; set; }

            public float Rate { get; set; } = 1f;
        }
    }
}
