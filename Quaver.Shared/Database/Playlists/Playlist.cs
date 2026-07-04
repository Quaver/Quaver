using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using Quaver.API.Enums;
using Quaver.API.Maps.Parsers;
using Quaver.API.Maps.Parsers.Stepmania;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
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
        ///     Exports the playlist to a directory
        /// </summary>
        public void Export(ExportMode exportMode)
        {
            var mapsetsAdded = new List<Mapset>();
            var mapsMd5 = new List<string>();

            var tempFolder = $"{ConfigManager.TempDirectory}/{GameBase.Game.TimeRunning}/";
            Directory.CreateDirectory(tempFolder);
            var exportsDirectory = $"{ConfigManager.DataDirectory}/Exports";
            Directory.CreateDirectory(exportsDirectory);
            
            var outputPath = $"{exportsDirectory}/{StringHelper.FileNameSafeString(Name)}.{(exportMode == ExportMode.Zip ? "zip" : "qpl")}";

            using (var archive = ZipArchive.Create())
            {
                foreach (var map in Maps)
                {
                    mapsMd5.Add(map.GetAlternativeMd5());
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
                        OnlineMapPoolId = OnlineMapPoolId,
                        OnlineMapPoolCreatorId = OnlineMapPoolCreatorId,
                        Maps = mapsMd5
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

            public int OnlineMapPoolId { get; set; }

            public int OnlineMapPoolCreatorId { get; set; }

            public List<string> Maps { get; set; }
        }
    }
}
