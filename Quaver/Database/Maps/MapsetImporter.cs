using System;
using System.IO;
using System.Linq;

using Quaver.Config;
using Quaver.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using Wobble;

namespace Quaver.Database.Maps
{
    public static class MapsetImporter
    {
        /// <summary>
        ///     If the import queue has maps ready to be imported.
        /// </summary>
        public static bool QueueReady { get; private set; }

        /// <summary>
        /// Watches the songs directory for any changes.
        /// </summary>
        internal static void WatchForChanges()
        {
            // Watch the song's directory for changes.
            var watcher = new FileSystemWatcher(ConfigManager.SongDirectory.Value)
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                               NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnDirectoryChange;
            watcher.Created += OnDirectoryChange;
            watcher.Deleted += OnDirectoryChange;
            watcher.Renamed += OnDirectoryChange;
        }

        /// <summary>
        /// If there were any changes in the directory, we'll make sure to set a flag that the maps
        /// need to be reprocessed later on.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnDirectoryChange(object source, FileSystemEventArgs e)
        {
            if (!QueueReady)
                Logger.LogInfo($"Detected directory change at: {e.FullPath}", LogType.Runtime);

            QueueReady = true;
        }

        /// <summary>
        ///     Responsible for extracting the files from the .qp
        /// </summary>
        /// <param name="fileName"></param>
        internal static void Import(string fileName)
        {
            var extractPath = $@"{ConfigManager.SongDirectory}/{Path.GetFileNameWithoutExtension(fileName)}/";

            try
            {

                using (var archive = ArchiveFactory.Open(fileName))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(extractPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Reloads the maps properly
        /// </summary>
        /// <returns></returns>
        internal static void AfterImport()
        {
            var oldMaps = MapManager.Mapsets;

            // Import all the maps to the db
            MapCache.LoadAndSetMapsets();

            // Update the selected map with the new one.
            // This button should only be on the song select state, so no need to check for states here.
            var newMapsets = MapManager.Mapsets.Where(x => oldMaps.All(y => y.Directory != x.Directory)).ToList();

            // In the event that the user imports maps when there weren't any maps previously.
            if (oldMaps.Count == 0)
            {
            }
            else if (newMapsets.Count > 0)
            {
                var map = newMapsets.Last().Maps.Last();
                map.ChangeSelected();
            }

            Logger.LogSuccess("Successfully completed the conversion task.", LogType.Runtime);

            QueueReady = false;
        }
    }
}