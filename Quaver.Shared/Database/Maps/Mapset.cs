/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Quaver.API.Maps.Parsers;
using Quaver.API.Maps.Parsers.Stepmania;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
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
        public string ExportToZip(bool openInExplorer = true)
        {
            var exportsDir = $"{ConfigManager.DataDirectory}/Exports/";
            System.IO.Directory.CreateDirectory(exportsDir);

            var tempFolder = $"{ConfigManager.TempDirectory}/{GameBase.Game.TimeRunning}/";
            System.IO.Directory.CreateDirectory(tempFolder);

            string outputPath = null;

            using (var archive = ZipArchive.Create())
            {
                foreach (var map in Maps)
                {
                    try
                    {
                        var folderPath = "";

                        switch (map.Game)
                        {
                            case MapGame.Quaver:
                                folderPath = $"{ConfigManager.SongDirectory.Value}/{map.Directory}";
                                var path = $"{folderPath}/{map.Path}";

                                File.Copy(path, $"{tempFolder}/{map.Path}", true);
                                break;
                            // Map is from osu!, so we need to convert it to .qua format
                            case MapGame.Osu:
                                folderPath = $"{MapManager.OsuSongsFolder}{map.Directory}";
                                var osuPath = $"{folderPath}/{map.Path}";

                                var osu = new OsuBeatmap(osuPath);
                                map.BackgroundPath = osu.Background;

                                var name = StringHelper.FileNameSafeString($"{map.Artist} - {map.Title} [{map.DifficultyName}].qua");
                                var savePath = $"{tempFolder}/{name}";

                                osu.ToQua().Save(savePath);

                                Logger.Debug($"Successfully converted osu beatmap: {osuPath}", LogType.Runtime);
                                break;
                            case MapGame.Etterna:
                                folderPath = Path.GetDirectoryName(map.Path);
                                var stepFile = new StepFile(map.Path);

                                var fileName = StringHelper.FileNameSafeString($"{map.Artist} - {map.Title} [{map.DifficultyName}].qua");
                                var fileSavePath = $"{tempFolder}/{fileName}";
                                stepFile.ToQuas().Find(x => x.DifficultyName == map.DifficultyName).Save(fileSavePath);

                                Logger.Debug($"Successfully converted stepmania chart: {map.Path}", LogType.Runtime);
                                break;
                        }

                        // Copy each non-map file in the directory. Handles things like audio files, backgrounds, etc.
                        foreach (var file in System.IO.Directory.GetFiles(folderPath))
                        {
                            switch (Path.GetExtension(file).ToLower())
                            {
                                // This list matches the extensions allowed by the server. If any other file is included,
                                // the server will reject attempts to upload the map.
                                case ".mp3":
                                case ".ogg":
                                case ".png":
                                case ".jpg":
                                case ".jpeg":
                                case ".wav":
                                    File.Copy(file, $"{tempFolder}/{Path.GetFileName(file)}", true);
                                    break;
                                default:
                                    continue;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                    }
                }

                archive.AddAllFromDirectory(tempFolder);

                outputPath = exportsDir +
                                 $"{StringHelper.FileNameSafeString(Artist + " - " + Title + " - " + GameBase.Game.TimeRunning)}.qp";

                archive.SaveTo(outputPath, CompressionType.Deflate);

                if (openInExplorer)
                    Utils.NativeUtils.HighlightInFileManager(outputPath);
            }

            System.IO.Directory.Delete(tempFolder, true);
            return outputPath;
        }
    }
}
