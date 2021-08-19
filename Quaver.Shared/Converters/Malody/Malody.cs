using Quaver.Shared.Config;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.IO;
using Newtonsoft.Json;
using Quaver.API.Maps.Parsers;
using Quaver.API.Maps.Parsers.Malody;
using Wobble.Logging;

namespace Quaver.Shared.Converters.Malody
{
    public static class Malody
    {
        /// <summary>
        /// </summary>
        public static void ExtractFile(string file, string extractDirectory)
        {
            var tempFolder = CreateTemp(file);

            foreach (var f in Directory.GetFiles(Path.GetDirectoryName(file)))
            {
                File.Copy(f, Path.Combine(tempFolder, Path.GetFileName(f)));
            }

            ConvertFiles(tempFolder, extractDirectory);
        }

        /// <summary>
        /// </summary>
        private static void ConvertFiles(string tempFolder, string extractDirectory)
        {
            foreach (var malFile in Directory.GetFiles(tempFolder, "*.mc", SearchOption.AllDirectories))
            {
                try
                {
                    var map = MalodyFile.Parse(malFile);

                    // Convert the map to .qua
                    map.ToQua().Save(Path.Combine(tempFolder, Path.GetFileName(malFile).Replace(".mc", ".qua")));
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

        /// <summary>
        /// </summary>
        private static string CreateTemp(string file)
        {
            var time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Milliseconds;
            var tempFolder = $@"{ConfigManager.TempDirectory}/{Path.GetFileNameWithoutExtension(file)} - {time}";

            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);

            Directory.CreateDirectory(tempFolder);

            return tempFolder;
        }

        /// <summary>
        /// </summary>
        public static void ExtractZip(string file, string extractDirectory)
        {
            var tempFolder = CreateTemp(file);

            using (var archive = ArchiveFactory.Open(file))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                        entry.WriteToDirectory(tempFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                }
            }

            ConvertFiles(tempFolder, extractDirectory);
        }
    }
}
