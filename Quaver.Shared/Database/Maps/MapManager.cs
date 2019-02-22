/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Parsers;
using Quaver.Shared.Config;
using Wobble.Bindables;

namespace Quaver.Shared.Database.Maps
{
    public static class MapManager
    {
        /// <summary>
        ///     The currently selected map.
        /// </summary>
        public static Bindable<Map> Selected { get; set; } = new Bindable<Map>(null);

        /// <summary>
        ///     The list of mapsets that are currently loaded.
        /// </summary>
        public static List<Mapset> Mapsets { get; set; }

        /// <summary>
        ///     The osu! Songs folder path
        /// </summary>
        public static string OsuSongsFolder { get; set; }

        /// <summary>
        ///     The current path of the selected map's audio file
        /// </summary>
        public static string CurrentAudioPath => GetAudioPath(Selected.Value);

        /// <summary>
        ///     The current background of the map.
        /// </summary>
        public static Texture2D CurrentBackground { get; set; }

        /// <summary>
        ///     The current path of the selected map's background path.
        /// </summary>
        public static string CurrentBackgroundPath => GetBackgroundPath(Selected.Value);

        /// <summary>
        ///     Gets the background path for a given map.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static string GetBackgroundPath(Map map)
        {
            switch (map.Game)
            {
                case MapGame.Osu:
                    // Parse the map and get the background
                    var osu = new OsuBeatmap(OsuSongsFolder + map.Directory + "/" + map.Path);
                    return $@"{OsuSongsFolder}/{map.Directory}/{osu.Background}";
                case MapGame.Quaver:
                    return ConfigManager.SongDirectory + "/" + map.Directory + "/" + map.BackgroundPath;
                default:
                    return "";
            }
        }

        /// <summary>
        ///     Gets a map's audio path taking into account the game.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static string GetAudioPath(Map map)
        {
            switch (map.Game)
            {
                case MapGame.Osu:
                    return OsuSongsFolder + "/" + map.Directory + "/" + map.AudioPath;
                case MapGame.Quaver:
                    return ConfigManager.SongDirectory + "/" + map.Directory + "/" + map.AudioPath;
                default:
                    return "";
            }
        }
    }
}
