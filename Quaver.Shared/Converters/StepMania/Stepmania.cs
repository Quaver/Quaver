/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using System.Linq;
using Quaver.API.Maps.Parsers;
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
                var quaFiles = StepManiaFile.Parse(file).ToQua();
                Directory.CreateDirectory(extractDirectory);

                for (var i = 0; i < quaFiles.Count; i++)
                {
                    var qua = quaFiles[i];
                    qua.Save($"{extractDirectory}/{i}.qua");
                }

                try
                {
                    var audioFile = quaFiles.First().AudioFile;
                    File.Copy($"{Path.GetDirectoryName(file)}/{audioFile}", $"{extractDirectory}/{audioFile}");
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }

                try
                {
                    var backgroundFile = quaFiles.First().BackgroundFile;
                    File.Copy($"{Path.GetDirectoryName(file)}/{backgroundFile}", $"{extractDirectory}/{backgroundFile}");
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}
