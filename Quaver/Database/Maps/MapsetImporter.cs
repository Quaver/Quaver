using System.IO;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.Database.Maps
{
    internal class MapsetImporter
    {
        /// <summary>
        /// Watches the songs directory for any changes.
        /// </summary>
        internal static void WatchForChanges()
        {
            // Watch the song's directory for changes.
            var watcher = new FileSystemWatcher(ConfigManager.SongDirectory)
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
        internal static void OnDirectoryChange(object source, FileSystemEventArgs e)
        {
            Logger.LogInfo($"Detected directory change at: {e.FullPath}", LogType.Runtime);
            GameBase.ImportQueueReady = true;
        }
    }
}
