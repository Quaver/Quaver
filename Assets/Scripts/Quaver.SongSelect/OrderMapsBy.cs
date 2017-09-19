using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Quaver.Cache;

namespace Quaver.SongSelect
{
    public class OrderMapsBy
    {
        // Responsible for sorting maps by their directory and returning a list of directories.
        // This essentially will be useful for song select, when the user is grouping by directory.
        // This will be the default option.
        public static List<MapDirectory> Directory(List<CachedBeatmap> loadedBeatmaps)
        {
            // Sort loaded beatmaps into directory
            loadedBeatmaps = loadedBeatmaps.OrderBy(x => x.Directory).ToList();

            List<MapDirectory> sortedMaps = new List<MapDirectory>();

            // For every map, we want to check every other map and create a list with that 
            foreach (CachedBeatmap loadedMap in loadedBeatmaps)
            {
                bool dirExists = false;

                foreach (MapDirectory mapSet in sortedMaps)
                {
                    if (mapSet.Directory == loadedMap.Directory)
                    {
                        dirExists = true;
                        break;
                    }
                }

                if (dirExists)
                    continue;

                // Go through all the other maps in the loaded maps, and find ones with the same directory.
                MapDirectory directory = new MapDirectory();
                directory.Beatmaps = new List<CachedBeatmap>();

                foreach (CachedBeatmap otherBeatmap in loadedBeatmaps)
                {
                    if (loadedMap.Directory == otherBeatmap.Directory)
                    {
                        directory.Directory = loadedMap.Directory;
                        directory.Beatmaps.Add(otherBeatmap);
                    }
                }

                sortedMaps.Add(directory);
            }

            return sortedMaps;
        }

        // This will take an existing MapDirectory and return a new one with the maps ordered by Artist
        // However, this will still order the maps in alphabetical order as well!
        public static List<MapDirectory> Artist(List<MapDirectory> beatmapSets)
        {
            return beatmapSets.OrderByDescending(set => set.Beatmaps[0].Artist).ThenByDescending(set => set.Beatmaps[0].Title).ToList();
        }

        // This will take an existing MapDirectory and return a new one with the maps ordered by Title
        public static List<MapDirectory> Title(List<MapDirectory> beatmapSets)
        {
            return beatmapSets.OrderByDescending(set => set.Beatmaps[0].Title).ToList();
        }

        // This will take an existing MapDirectory and return a new one with the maps ordered by Creator
        // but also orders the maps in alphabetical order by title as well!
        public static List<MapDirectory> Creator(List<MapDirectory> beatmapSets)
        {
            return OrderMapsBy.Title(beatmapSets.OrderByDescending(set => set.Beatmaps[0].Creator).ToList());
        }
    }
}
