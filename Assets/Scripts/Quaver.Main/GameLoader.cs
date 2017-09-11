using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Quaver.Cache;
using Quaver.Config;
using Quaver.SongSelect;

public class GameLoader : MonoBehaviour 
{
	public Cfg UserConfig;
	public List<CachedBeatmap> LoadedBeatmaps = new List<CachedBeatmap>();

	void Awake()
	{
		// Create the beatmap database if it doesn't already exist.
		BeatmapCacheIndex.CreateDatabase();

		// Load the User Config - If it doesn't already exist, it will generate one
		// and all the necessary files.
		UserConfig = ConfigLoader.Load();

		// Load Beatmaps into the list of LoadedBeatmaps
		LoadedBeatmaps = BeatmapCacheIndex.LoadBeatmaps(UserConfig);

		// Get the list of maps ordered by directory.
		// These are the beatmaps that will be used during song select.
		List<MapDirectory> mapDirectories = OrderMapsBy.Directory(LoadedBeatmaps);
		Debug.Log("[CACHE] There were: " + LoadedBeatmaps.Count + " beatmaps in " + mapDirectories.Count + " directories loaded.");
	}
}
