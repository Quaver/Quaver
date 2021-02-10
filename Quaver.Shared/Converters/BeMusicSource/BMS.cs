using System;
using System.IO;
using System.Linq;
using MoreLinq.Extensions;
using Quaver.API.Maps.Parsers.BeMusicSource;
using Quaver.Shared.Config;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using Wobble.Logging;

namespace Quaver.Shared.Converters.BeMusicSource
{
    public class BMS
    {
        private string rootDir;

        public void ConvertBMS(string zipFile, string extractionDirectory)
        {
            var time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;
            var tempFolder = $@"{ConfigManager.TempDirectory}/{Path.GetFileNameWithoutExtension(zipFile)} - {time}";

            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);

            using (var archive = ArchiveFactory.Open(zipFile))
            {
                var reader = archive.ExtractAllEntries();
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(tempFolder,
                            new ExtractionOptions() {Overwrite = true, ExtractFullPath = true});
                    }
                }
            }

            foreach (var bmsFile in "*.bms|*.bme|*.bml"
                .Split("|")
                .SelectMany(filter => Directory.EnumerateFiles(tempFolder, filter, SearchOption.AllDirectories))
                .ToArray()
            )
            {
                try
                {
                    var map = new BMSFile(bmsFile);

                    if (!string.IsNullOrEmpty(map.InvalidReason))
                    {
                        Logger.Debug($"BMS file {Path.GetFileName(bmsFile)} is invalid: {map.InvalidReason}", LogType.Runtime);
                        continue;
                    }

                    // Convert the map to .qua
                    var toQua = map.ToQua();

                    if (!toQua.IsValid())
                    {
                        continue;
                    }

                    toQua.Save(Path.Combine(tempFolder, Path.GetFileName(bmsFile)
                        .Replace(".bms", ".qua")
                        .Replace(".bme", ".qua")
                        .Replace(".bml", ".qua"))
                    );
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            // Check if the directory structure looks like /rootdir/content instead of /content
            var dirs = Directory.EnumerateDirectories(tempFolder).ToArray();
            if (dirs.Length > 0)
            {
                var f = Directory.EnumerateFiles(dirs[0], "*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.EndsWith(".bms") || s.EndsWith(".bme") || s.EndsWith(".bml"));
                if (dirs.Length == 1 && f.Any())
                {
                    rootDir = Path.GetFileName(dirs[0]);
                }
            }

            Directory.CreateDirectory(extractionDirectory);

            var dirCheck = Directory.EnumerateDirectories(Path.Join(tempFolder, rootDir), "*", SearchOption.AllDirectories);
            dirCheck.ForEach(dir =>
            {
                Directory.CreateDirectory(Path.Join(extractionDirectory, Path.GetRelativePath(Path.Join(tempFolder, rootDir), dir)));
            });

            // lol
            foreach (var tempFile in Directory.EnumerateFiles(tempFolder, "*", SearchOption.AllDirectories))
            {
                var scopedFileName = Path.GetRelativePath(tempFolder, tempFile);
                var edited = rootDir == null ? scopedFileName : scopedFileName.Replace(rootDir, "");
                var fileName = Path.Join(extractionDirectory, edited);

                while (fileName.Length > 256)
                {
                    fileName = fileName.Remove(fileName.Length - 1);
                }

                switch (Path.GetExtension(tempFile).ToLower())
                {
                    case ".qua":
                    case ".mp3":
                    case ".wav":
                    case ".ogg":
                    case ".3gp":
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                        File.Move(tempFile, fileName);
                        break;
                    default:
                        continue;
                }
            }
        }
    }
}