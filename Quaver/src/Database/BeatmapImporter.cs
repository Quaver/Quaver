﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Config;
using Quaver.Main;

namespace Quaver.Database
{
    internal class BeatmapImporter
    {
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
            GameBase.ImportQueueReady = true;
        }
    }
}
