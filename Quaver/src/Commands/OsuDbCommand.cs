using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using osu.Shared;
using osu_database_reader.BinaryFiles;
using Quaver.Logging;
using Quaver.Maps;
using Quaver.Peppy;
using Quaver.Utility;
using Configuration = Quaver.Config.Configuration;

namespace Quaver.Commands
{
    internal class OsuDbCommand : ICommand
    {
        public string Name { get; set; } = "OSUDB";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Reads an osu!.db file";

        public string Usage { get; set; } = "osudb <path to osu!.db>";

        public void Execute(string[] args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);
            var path = string.Join(" ", argsList);

            try
            {
                var db = OsuDb.Read(path);
                var beatmaps = db.Beatmaps.Where(x => x.GameMode == GameMode.Mania).ToList();
                Console.WriteLine($"Detected: {beatmaps.Count} osu!mania beatmaps");

                var setsConverted = new List<int>();

                foreach (var map in beatmaps)
                {
                    try
                    {
                        // Skip if the mapset was already converted.
                        if (setsConverted.Contains(map.BeatmapSetId))
                            continue;

                        // Create a folder in the songs directory w/ the same name
                        var dir = Configuration.SongDirectory + "/" + map.FolderName;
                        Directory.CreateDirectory(dir);

                        // Start converting
                        var osuMapDir = Path.GetDirectoryName(path) + "/Songs/" + map.FolderName;

                        // Get all the files in the folder
                        var files = Directory.GetFiles(osuMapDir);

                        foreach (var file in files)
                        {
                            if (file.EndsWith(".ogg") || file.EndsWith(".mp3") ||
                                file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
                            {
                                File.Copy(file,
                                    dir + "/" + Util.FileNameSafeString(Path.GetFileName(file).Replace("\\", "/")));
                            }

                            
                            if (file.EndsWith(".osu"))
                            {
                                Console.WriteLine(file);
                                var osu = new PeppyBeatmap(osuMapDir + "/" + Path.GetFileName(file));

                                if (osu.Mode != 3)
                                    continue;

                                var qua = Qua.ConvertOsuBeatmap(osu);
                                qua.Save(dir + "/" + Util
                                             .FileNameSafeString(Path.GetFileName(file).Replace(".osu", ".qua"))
                                             .Replace("\\", "/"));
                            }
                        }

                        setsConverted.Add(map.BeatmapSetId);
                    }
                    catch (Exception e)
                    {
                        setsConverted.Add(map.BeatmapSetId);
                        Logger.Log(e.Message, LogColors.GameError);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
