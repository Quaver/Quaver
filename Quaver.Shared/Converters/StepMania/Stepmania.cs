/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using System.Linq;
using Quaver.API.Maps.Parsers;
using Quaver.API.Maps.Parsers.StepMania;
using Quaver.Shared.Config;
using Wobble.Logging;

namespace Quaver.Shared.Converters.StepMania
{
    public static class Stepmania
    {
        /// <summary>
        ///    Handles the conversion of a single .sm file
        /// </summary>
        public static void ConvertFile(string file, string extractDirectory)
        {
            var time = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;
            var tempFolder = $@"{ConfigManager.DataDirectory}/Temp/{Path.GetFileNameWithoutExtension(file)} - {time}";

            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);

            try
            {
                var quaFiles = new StepmaniaConverter(file).ToQua();
                Directory.CreateDirectory(extractDirectory);

                for (var i = 0; i < quaFiles.Count; i++)
                {
                    var qua = quaFiles[i];

                    // Apparently sm can have backgrounds defaulted to files that end in -bg or just bg.
                    // this handles the case of if the background file is initially null in the .sm file
                    // and sets the appropriate background file to use.
                    if (string.IsNullOrEmpty(qua.BackgroundFile))
                    {
                        foreach (var f in Directory.GetFiles(Path.GetDirectoryName(file)))
                        {
                            var fileName = f.ToLower();

                            if (!fileName.EndsWith("bg.jpg") && !fileName.EndsWith("bg.jpeg") && !fileName.EndsWith("bg.png"))
                                continue;

                            qua.BackgroundFile = Path.GetFileName(f);
                            break;
                        }
                    }

                    qua.Save($"{extractDirectory}/{i}.qua");
                }

                foreach (var qua in quaFiles)
                {
                    // Copy over audio files
                    try
                    {
                        var path = $"{extractDirectory}/{qua.AudioFile}";

                        if (!File.Exists(path))
                            File.Copy($"{Path.GetDirectoryName(file)}/{qua.AudioFile}", path);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                    }

                    // Copy over background files.
                    try
                    {
                        var path = $"{extractDirectory}/{qua.BackgroundFile}";

                        if (!File.Exists(path))
                            File.Copy($"{Path.GetDirectoryName(file)}/{qua.BackgroundFile}", path);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
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
