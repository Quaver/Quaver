using System.Collections.Generic;
using Quaver.API.Maps.Parsers;
using Quaver.Config;

namespace Quaver.Database.Maps
{
    public static class MapManager
    {
        /// <summary>
        ///     The currently selected map.
        /// </summary>
        public static Map Selected { get; set; }

        /// <summary>
        ///     The list of mapsets that are currently loaded.
        /// </summary>
        public static List<Mapset> Mapsets { get; set; }

        /// <summary>
        ///     The osu! Songs folder path
        /// </summary>
        public static string OsuSongsFolder { get; set; }

        /// <summary>
        ///     The Etterna parent folder.
        ///     NOTE: The Map directory themselves have /songs/ already on it.
        ///     Thank you SM guys!
        /// </summary>
        public static string EtternaFolder { get; set; }

        /// <summary>
        ///     The current path of the selected map's audio file
        /// </summary>
        public static string CurrentAudioPath
        {
            get
            {
                switch (Selected.Game)
                {
                    case MapGame.Osu:
                        return OsuSongsFolder + "/" + Selected.Directory + "/" + Selected.AudioPath;
                    case MapGame.Quaver:
                        return ConfigManager.SongDirectory + "/" + Selected.Directory + "/" + Selected.AudioPath;
                    case MapGame.Etterna:
                        return EtternaFolder + "/" + Selected.Directory + "/" + Selected.AudioPath;
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        ///     The current path of the selected map's background path.
        /// </summary>
        public static string CurrentBackgroundPath
        {
            get
            {
                switch (Selected.Game)
                {
                    case MapGame.Osu:
                        // Parse the map and get the background
                        var osu = new OsuBeatmap(OsuSongsFolder + Selected.Directory + "/" + Selected.Path);
                        return $@"{OsuSongsFolder}/{Selected.Directory}/{osu.Background}";
                    case MapGame.Quaver:
                        return ConfigManager.SongDirectory + "/" + Selected.Directory + "/" + Selected.BackgroundPath;
                    case MapGame.Etterna:
                        return EtternaFolder + "/" + Selected.Directory + "/" + Selected.BackgroundPath;
                    default:
                        return "";
                }
            }
        }
    }
}