using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Cache;

namespace Quaver.SongSelect
{
	public struct MapDirectory
	{
		public string Directory;
		public List<CachedBeatmap> Beatmaps;
	}
}
