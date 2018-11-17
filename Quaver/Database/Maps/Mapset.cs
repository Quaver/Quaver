using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Quaver.API.Maps.Parsers;
using Quaver.Config;
using Quaver.Helpers;
using Quaver.Scheduling;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using WebSocketSharp;
using Wobble;
using Wobble.Logging;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Database.Maps
{
    public class Mapset
    {
        /// <summary>
        ///     The directory of the mapset.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        ///     The list of maps in this mapset.
        /// </summary>
        public List<Map> Maps { get; set; }

        /// <summary>
        ///     The last selected/preferred map in this set
        /// </summary>
        public Map PreferredMap { get; set; }

        public string Artist => Maps.First().Artist;
        public string Title => Maps.First().Title;
        public string Creator => Maps.First().Creator;
        public string Background => MapManager.GetBackgroundPath(Maps.First());

        /// <summary>
        ///     Exports the entire mapset to a zip (.qp) file.
        /// </summary>
        public void ExportToZip()
        {
            System.IO.Directory.CreateDirectory($"{ConfigManager.DataDirectory}/Exports/");

            var tempFolder = $"{ConfigManager.DataDirectory}/temp/{GameBase.Game.TimeRunning}/";
            System.IO.Directory.CreateDirectory(tempFolder);

            using (var archive = ZipArchive.Create())
            {
                foreach (var map in Maps)
                {
                    var path = "";

                    try
                    {
                        switch (map.Game)
                        {
                            case MapGame.Quaver:
                                path = $"{ConfigManager.SongDirectory.Value}/{map.Directory}/{map.Path}";
                                break;
                            // Map is from osu!, so we need to convert it to .qua format
                            case MapGame.Osu:
                                var osuPath = $"{MapManager.OsuSongsFolder}/{map.Directory}/{map.Path}";

                                Logger.Debug($"Need to convert osu beatmap at path: ${osuPath} to .qua...", LogType.Runtime);

                                var qua = new OsuBeatmap(osuPath).ToQua();

                                path = $"{tempFolder}/{map.Artist} - ${map.Title} [${map.DifficultyName}]";
                                qua.Save(StringHelper.FileNameSafeString(path));

                                Logger.Debug($"Successfully converted osu beatmap: ${osuPath}", LogType.Runtime);
                                break;
                        }


                        File.Copy(path, $"{tempFolder}/{map.Path}");

                        // Copy over audio file if necessary
                        if (File.Exists(MapManager.GetAudioPath(map)) && !File.Exists($"{tempFolder}/{map.AudioPath}"))
                            File.Copy(MapManager.GetAudioPath(map), $"{tempFolder}/{map.AudioPath}");

                        // Copy over background file if necessary
                        if (File.Exists(MapManager.GetBackgroundPath(map)) && !File.Exists($"{tempFolder}/{map.BackgroundPath}"))
                            File.Copy(MapManager.GetBackgroundPath(map), $"{tempFolder}/{map.BackgroundPath}");
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                    }
                }

                archive.AddAllFromDirectory(tempFolder);

                var outputPath = $"{ConfigManager.DataDirectory}/Exports/" +
                                 $"{StringHelper.FileNameSafeString(Artist + " " + Title + " " + GameBase.Game.TimeRunning)}.qp";

                archive.SaveTo(outputPath, CompressionType.Deflate);

                Process.Start("explorer.exe", "/select, \"" + outputPath.Replace("/", "\\") + "\"");
            }

            System.IO.Directory.Delete(tempFolder, true);
        }
    }
}