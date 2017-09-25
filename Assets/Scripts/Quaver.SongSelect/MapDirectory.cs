using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Cache;

namespace Quaver.SongSelect
{
    public struct MapDirectory
    {
        /// <summary>
        /// The path of the directory that holds the beatmaps.
        /// </summary>
        public string Directory;

        /// <summary>
        /// The list of cached beatmaps inside of this directory.
        /// </summary>
        public List<CachedBeatmap> Beatmaps;
    }
}
