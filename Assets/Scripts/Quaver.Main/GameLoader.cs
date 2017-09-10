using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Cache;
using Quaver.Config;

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
		LoadedBeatmaps = BeatmapCacheIndex.LoadBeatmaps();
		Debug.Log("[CACHE] Beatmaps Loaded: " + LoadedBeatmaps.Count);

	}
}
