using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Quaver.Cache
{
	public class CachedBeatmap
	{
		public string DirectoryMD5;
		public string PathMD5;
		public int BeatmapSetID;
		public int BeatmapID;
		public string Artist;
		public string Title;
		public float Difficulty;
		public string Rank;
		public int Status;
		public DateTime LastPlayed;

		// Set all the cached beatmap values in the constructor
		public CachedBeatmap(string directoryMD5, string pathMD5, int beatmapSetID, int beatmapID, string artist,
							string title, float difficulty, string rank, int status, DateTime lastPlayed)
		{
			this.DirectoryMD5 = directoryMD5;
			this.PathMD5 = pathMD5;
			this.BeatmapSetID = beatmapSetID;
			this.BeatmapID = beatmapID;
			this.Artist = artist;
			this.Title = title;
			this.Difficulty = difficulty;
			this.Rank = rank;
			this.Status = status;
			this.LastPlayed = lastPlayed;
		}

	}
}

