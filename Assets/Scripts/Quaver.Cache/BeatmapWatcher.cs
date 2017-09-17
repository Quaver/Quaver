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
