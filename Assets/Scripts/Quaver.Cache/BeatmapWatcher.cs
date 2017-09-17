using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;

namespace Quaver.Cache
{
	public class BeatmapWatcher
	{
		public static void Watch(Cfg userConfig)
		{
			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = userConfig.SongDirectory;
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Filter = "*.*";
			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.EnableRaisingEvents = true;
		}

		private static void OnChanged(object source, FileSystemEventArgs e)
		{
			Debug.LogWarning("Change detected in songs directory!!");
		}
	}
}
