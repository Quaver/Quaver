using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Config;

namespace Quaver.Database
{
    internal class BeatmapImporter
    {
        /// <summary>
        /// Global property that keeps track of whether or not we have beatmap changes queued.
        /// This is set to true whenever a change in the directory has been detected. 
        /// </summary>
        internal static bool changesQueued = false;

        /// <summary>
        /// Watches the songs directory for any changes.
        /// </summary>
        internal static void WatchForChanges()
        {
            // Watch the song's directory for changes.
            var watcher = new FileSystemWatcher(Configuration.SongDirectory)
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                               NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnDirectoryChange;
            watcher.Created += OnDirectoryChange;
            watcher.Deleted += OnDirectoryChange;
        }

        /// <summary>
        /// If there were any changes in the directory, we'll make sure to set a flag that the maps
        /// need to be reprocessed later on.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        internal static void OnDirectoryChange(object source, FileSystemEventArgs e)
        {
            changesQueued = true;
        }
    }
}
