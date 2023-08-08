/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using Quaver.API.Maps.Parsers;
using Quaver.Shared.Config;
using SharpCompress.Archives;
using SharpCompress.Common;
using Wobble.Logging;

namespace Quaver.Shared.Converters.Osu
{
    public static class Osu
    {
        /// <summary>
        ///     Converts a .osz archive file to .qua format. M
        ///     (Makes it available to play in Quaver)
        /// </summary>
        public static void ConvertOsz(string file, string extractDirectory)
        {
            var time = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;
            var tempFolder = $@"{ConfigManager.TempDirectory}/{Path.GetFileNameWithoutExtension(file)} - {time}";

            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);

            Directory.CreateDirectory(tempFolder);

            try
            {
                // Extract the .osz archive to the temporary folder
                using (var archive = ArchiveFactory.Open(file))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                            entry.WriteToDirectory(tempFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    }
                }

                // Convert all .osu files in the temp folder to .qua
                foreach (var osuFile in Directory.GetFiles(tempFolder, "*.osu", SearchOption.AllDirectories))
                {
                    try
                    {
                        var map = new OsuBeatmap(osuFile);

                        if (!map.IsValid)
                            continue;

                        // Convert the map to .qua
                        map.ToQua().Save(map.OriginalFileName.Replace(".osu", ".qua"));
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                    }
                }

                Directory.CreateDirectory(extractDirectory);

                // Go through each file in the temp folder and add it to the target directory
                foreach (var tempFile in Directory.GetFiles(tempFolder))
                {
                    var fileName = $"{extractDirectory}/{Path.GetFileName(tempFile)}";

                    // Make sure the path to the file is less than 260 characters
                    while (fileName.Length > 260)
                        fileName = fileName.Remove(fileName.Length - 1);

                    // Go through each file and move it.
                    switch (Path.GetExtension(tempFile).ToLower())
                    {
                        case ".qua":
                            File.Move(tempFile, $"{extractDirectory}/{Guid.NewGuid()}.qua");
                            break;
                        case ".mp3":
                        case ".jpg":
                        case ".png":
                        case ".jpeg":
                        case ".ogg":
                        case ".wav":
                            File.Move(tempFile, fileName);
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
    }
}
