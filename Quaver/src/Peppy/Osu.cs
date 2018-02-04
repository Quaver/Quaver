﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Osu;
using Quaver.Logging;

namespace Quaver.Peppy
{
    internal static class Osu
    {
        /// <summary>
        ///     Responsible for converting a .osz file to a new song directory full of .qua
        /// </summary>
        /// <param name="fileName"></param>
        internal static void ConvertOsz(string fileName, int num)
        {
            // Extract the .osu & relevant audio files, and attempt to convert them.
            // Once fully converted, create a new directory in the songs folder and 
            // tell GameBase that the import queue is ready. Depending on the current state,
            // we may import them automatically.
            var extractPath = $@"{Config.Configuration.DataDirectory}/Temp/{num}";

            try
            {
                using (var archive = new ZipFile(fileName))
                {
                    archive.ExtractAll(extractPath, ExtractExistingFileAction.OverwriteSilently);
                }

                // Now that we have them, proceed to convert them.
                foreach (var file in Directory.GetFiles(extractPath, "*.osu", SearchOption.AllDirectories))
                {
                    var map = new PeppyBeatmap(file);

                    if (!map.IsValid)
                        continue;

                    // Convert the map to .qua
                    var qua = Qua.ConvertOsuBeatmap(map);
                    qua.Save(map.OriginalFileName.Replace(".osu", ".qua"));
                }

                // Now that all of them are converted, we'll create a new directory with all of the files except for .osu

                var newSongDir = $"{Config.Configuration.SongDirectory}/{new DirectoryInfo(fileName).Name}";

                if (newSongDir.Length > 200)
                    newSongDir =
                        $"{Config.Configuration.SongDirectory}/{new DirectoryInfo(fileName).Name.Substring(0, 20)}";

                Directory.CreateDirectory(newSongDir);

                // Get the files that are currently in the extract path
                var filesInDir = Directory.GetFiles(extractPath);

                for (var i = 0; i < filesInDir.Length; i++)
                {
                    switch (Path.GetExtension(filesInDir[i]))
                    {
                        case ".osu":
                            // Ignore .osu files
                            continue;
                        case ".qua":
                            // Try to create a similar path to the original. 
                            // The reason we generate all these new file names is because
                            // the path may end up being too long, and that throws an error.
                            var newFile = $"{newSongDir}/{Path.GetFileName(filesInDir[i])}";

                            if (newFile.Length > 200)
                                newFile = $"{newSongDir}/{Path.GetFileName(filesInDir[i]).Substring(0, 60)}.qua";

                            if (newFile.Length > 200 || File.Exists(newFile))
                                newFile = $"{newSongDir}/{i}.qua";

                            File.Move(filesInDir[i], newFile);
                            break;
                        // We only allow certain file to be moved over, no .wav files, as those are usually
                        // hitsounds from osu!
                        case ".mp3":
                        case ".jpg":
                        case ".png":
                        case ".jpeg":
                        case ".ogg":
                            File.Move(filesInDir[i], $"{newSongDir}/{Path.GetFileName(filesInDir[i])}");
                            break;
                    }
                }

                Logger.Log($".osz has been successfully converted.", LogColors.GameSuccess, 2f);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Log($"Error: There was an issue converting the .osz", LogColors.GameError, 2f);
                Logger.Log(e.Message, Color.Red);
            }
            // Delete the entire temp directory regardless of the outcome.
            finally
            {
                Directory.Delete(extractPath, true);
            }
        }
    }
}
