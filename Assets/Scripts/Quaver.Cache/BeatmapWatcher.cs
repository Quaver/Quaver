using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;
using Quaver.Main;

namespace Quaver.Cache
{
	public class BeatmapWatcher
	{
        /// <summary>
        /// Watches the Songs directory for any changes, and reports an event.
        /// </summary>
        /// <param name="userConfig">The user configuration object</param>
		public static void Watch(Cfg userConfig)
		{
			FileSystemWatcher watcher = new FileSystemWatcher(userConfig.SongDirectory);

			watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
            | NotifyFilters.FileName |NotifyFilters.DirectoryName;

			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Created += new FileSystemEventHandler(OnChanged);
			watcher.Deleted += new FileSystemEventHandler(OnChanged);
			watcher.EnableRaisingEvents = true;
		}

        /// <summary>
        /// Adds the changed directory to the queue of changes. 
        /// It wont be directory changed until the user presses F5.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="e">All the other cool stuff we get with the change</param>
		private static void OnChanged(object source, FileSystemEventArgs e)
		{
			FileInfo file = new FileInfo(e.FullPath);
			WatcherChangeTypes wct = e.ChangeType;

			Debug.LogWarning("[BEATMAP WATCHER] Detected Change Type: " + wct.ToString() + " on Directory: " + file.Name);

			// Add the song directory to the queue.
			if (!GameStateManager.SongDirectoryChangeQueue.Contains(file.Name))
			{
				GameStateManager.SongDirectoryChangeQueue.Add(file.Name);
				Debug.Log("Press F5 to refresh beatmaps.");
				Debug.Log(GameStateManager.SongDirectoryChangeQueue.Count);
			}
		}
	}
}
