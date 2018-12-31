/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Quaver.API.Maps.Parsers;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using Wobble;
using Wobble.Logging;
using Wobble.Platform;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Shared.Database.Maps
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
            var exportsDir = $"{ConfigManager.DataDirectory}/Exports/";
            System.IO.Directory.CreateDirectory(exportsDir);

            var tempFolder = $"{ConfigManager.DataDirectory}/temp/{GameBase.Game.TimeRunning}/";
            System.IO.Directory.CreateDirectory(tempFolder);

            using (var archive = ZipArchive.Create())
            {
                foreach (var map in Maps)
                {
                    try
                    {
                        switch (map.Game)
                        {
                            case MapGame.Quaver:
                                var path = $"{ConfigManager.SongDirectory.Value}/{map.Directory}/{map.Path}";
                                File.Copy(path, $"{tempFolder}/{map.Path}");
                                break;
                            // Map is from osu!, so we need to convert it to .qua format
                            case MapGame.Osu:
                                var osuPath = $"{MapManager.OsuSongsFolder}{map.Directory}/{map.Path}";

                                var osu = new OsuBeatmap(osuPath);
                                map.BackgroundPath = osu.Background;

                                var name = StringHelper.FileNameSafeString($"{map.Artist} - {map.Title} [{map.DifficultyName}].qua");
                                var savePath = $"{tempFolder}/{name}";

                                osu.ToQua().Save(savePath);

                                Logger.Debug($"Successfully converted osu beatmap: {osuPath}", LogType.Runtime);
                                break;
                        }

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

                var outputPath = exportsDir +
                                 $"{StringHelper.FileNameSafeString(Artist + " - " + Title + " - " + GameBase.Game.TimeRunning)}.qp";

                archive.SaveTo(outputPath, CompressionType.Deflate);

                Utils.NativeUtils.HighlightInFileManager(outputPath);
            }

            System.IO.Directory.Delete(tempFolder, true);
        }
    }
}
