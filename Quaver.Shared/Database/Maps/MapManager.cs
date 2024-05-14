/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emik;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Parsers;
using Quaver.Server.Client;
using Quaver.Server.Common.Objects.Twitch;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online.API.Maps;
using Quaver.Shared.Screens.Selection.UI.Maps;
using RestSharp;
using RestSharp.Extensions;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

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
        public static List<Mapset> Mapsets { get; set; } = new List<Mapset>();

        /// <summary>
        ///     List of recently selected/played maps
        /// </summary>
        public static List<Map> RecentlyPlayed { get; set; } = new List<Map>();

        /// <summary>
        ///     The osu! Songs folder path
        /// </summary>
        public static string OsuSongsFolder { get; set; }

        /// <summary>
        ///     The current path of the selected map's audio file
        /// </summary>
        public static string CurrentAudioPath => GetAudioPath(Selected?.Value);

        /// <summary>
        ///     The current background of the map.
        /// </summary>
        public static Texture2D CurrentBackground { get; set; }

        /// <summary>
        ///     The current path of the selected map's background path.
        /// </summary>
        public static string CurrentBackgroundPath => GetBackgroundPath(Selected.Value);

        /// <summary>
        ///     Event invoked when a mapset has been deleted
        /// </summary>
        public static event EventHandler<MapsetDeletedEventArgs> MapsetDeleted;

        /// <summary>
        ///     Event invoked when a map has been deleted
        /// </summary>
        public static event EventHandler<MapDeletedEventArgs> MapDeleted;

        /// <summary>
        ///     Event invoked when a map has been updated
        /// </summary>
        public static event EventHandler<MapUpdatedEventArgs> MapUpdated;

        /// <summary>
        ///     Event invoked when a song request has been played
        /// </summary>
        public static event EventHandler<SongRequestPlayedEventArgs> SongRequestPlayed;

        /// <summary>
        ///     Select a map in the mapset based on user preferences
        /// </summary>
        public static void SelectMapFromMapset(Mapset mapset)
        {
            // local function to select map closest to target difficulty
            Map Select(List<Map> maps, ModIdentifier mods)
            {
                if (maps.Count == 0) return null;

                // target difficulty
                double target4K = ConfigManager.PrioritizedMapDifficulty4K.Value / 10d;
                double target7K = ConfigManager.PrioritizedMapDifficulty7K.Value / 10d;

                // find closest map to target
                var minDelta = Double.PositiveInfinity;
                Map selection = null;

                foreach (var map in maps)
                {
                    var target = map.Mode switch
                    {
                        GameMode.Keys4 => target4K,
                        GameMode.Keys7 => target7K,
                        _ => throw new InvalidOperationException("Map is an invalid game mode")
                    };

                    double delta = Math.Abs(map.DifficultyFromMods(mods) - target);

                    if (delta < minDelta)
                    {
                        selection = map;
                        minDelta = delta;
                    }
                }

                return selection;
            }

            // prioritize a gamemode
            List<Map> prioritized = mapset.Maps.FindAll(x => x.Mode == ConfigManager.PrioritizedGameMode.Value);

            // select a map from the prioritized maps
            // if no valid selection from prioritized maps, select from all maps instead
            Selected.Value = Select(prioritized, ModManager.Mods) ?? Select(mapset.Maps, ModManager.Mods);
        }

        /// <summary>
        ///     Gets the background path for a given map.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static string GetBackgroundPath(Map map)
        {
            if (map == null)
                return "";

            switch (map.Game)
            {
                case MapGame.Osu:
                    // Parse the map and get the background
                    var osu = new OsuBeatmap(OsuSongsFolder + map.Directory + "/" + map.Path);
                    return $@"{OsuSongsFolder}/{map.Directory}/{osu.Background}";
                case MapGame.Quaver:
                    return ConfigManager.SongDirectory + "/" + map.Directory + "/" + map.BackgroundPath;
                case MapGame.Etterna:
                    return map.BackgroundPath;
                default:
                    return "";
            }
        }

        /// <summary>
        ///     Gets the banner path for a given map.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static string GetBannerPath(Map map)
        {
            if (map == null)
                return "";

            switch (map.Game)
            {
                case MapGame.Osu:
                    return "";
                case MapGame.Quaver:
                    return ConfigManager.SongDirectory + "/" + map.Directory + "/" + map.BannerPath;
                case MapGame.Etterna:
                    return map.BannerPath;
                default:
                    return "";
            }
        }

        /// <summary>
        ///     Returns the path of the banner file provided a mapset
        /// </summary>
        /// <param name="mapset"></param>
        /// <returns></returns>
        public static string GetMapsetBannerPath(Mapset mapset)
        {
            var map = mapset.Maps.First();

            switch (map.Game)
            {
                case MapGame.Osu:
                    return "";
                case MapGame.Quaver:
                    return (ConfigManager.SongDirectory + "/" + map.Directory + "/" + map.BannerPath).Replace("\\", "/");
                case MapGame.Etterna:
                    return map.BannerPath;
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
            if (map == null)
                return "";

            switch (map.Game)
            {
                case MapGame.Osu:
                    return OsuSongsFolder + "/" + map.Directory + "/" + map.AudioPath;
                case MapGame.Quaver:
                    return ConfigManager.SongDirectory + "/" + map.Directory + "/" + map.AudioPath;
                case MapGame.Etterna:
                    return map.AudioPath;
                default:
                    return "";
            }
        }

        /// <summary>
        ///     Finds a map based on the md5 hash
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        public static Map FindMapFromMd5(string md5)
        {
            foreach (var set in Mapsets)
            {
                var found = set.Maps.Find(x => x.Md5Checksum == md5);

                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        ///     Finds a map based on its online id
        /// </summary>
        /// <returns></returns>
        public static Map FindMapFromOnlineId(int id)
        {
            foreach (var set in Mapsets)
            {
                var found = set.Maps.Find(x => x.MapId == id);

                if (found != null)
                    return found;
            }

            return null;
        }

        ///<summary>
        ///     Gets a map's custom audio sample path taking into account the game.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="samplePath"></param>
        /// <returns></returns>
        public static string GetCustomAudioSamplePath(Map map, string samplePath)
        {
            switch (map.Game)
            {
                case MapGame.Osu:
                    return OsuSongsFolder + "/" + map.Directory + "/" + samplePath;
                case MapGame.Quaver:
                    return ConfigManager.SongDirectory + "/" + map.Directory + "/" + samplePath;
                default:
                    return "";
            }
        }

        /// <summary>
        ///     Views the online listing page on the website
        /// </summary>
        public static void ViewOnlineListing(Map map = null)
        {
            if (map == null)
                map = Selected.Value;

            if (map == null)
                return;

            if (map.MapId == -1)
            {
                NotificationManager.Show(NotificationLevel.Error, "This map is not submitted online!");
                return;
            }

            BrowserHelper.OpenURL($"https://quavergame.com/mapsets/map/{map.MapId}");
        }

        /// <summary>
        ///     Deletes a m map from the game
        /// </summary>
        /// <param name="map"></param>
        /// <param name="index"></param>
        public static void Delete(Map map, int index)
        {
            if (map.Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot delete a map loaded from another game");
                return;
            }

            try
            {
                var mapsetPath = Path.Combine(ConfigManager.SongDirectory.Value, map.Mapset.Directory);
                var path = Path.Combine(mapsetPath, map.Path);

                if (!Rubbish.Move(path))
                    ShowFallbackMapDeletionDialog(
                        "map",
                        () =>
                        {
                            File.Delete(path);
                            MapDatabaseCache.RemoveMap(map);
                        }
                    );
                else
                    MapDatabaseCache.RemoveMap(map);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            map.Mapset.Maps.Remove(map);

            // Delete the mapset entirely if there are no more maps left.
            if (map.Mapset.Maps.Count == 0)
                Mapsets.Remove(map.Mapset);

            PlaylistManager.RemoveMapFromAllPlaylists(map);

            // Raise an event with the deleted map
            MapDeleted?.Invoke(typeof(MapManager), new MapDeletedEventArgs(map, index));
        }

        /// <summary>
        ///     Deletes the mapset from the game
        /// </summary>
        public static void Delete(Mapset mapset, int index)
        {
            if (mapset.Maps.Count == 0)
                return;

            if (mapset.Maps.First().Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot delete a mapset loaded from another game");
                return;
            }

            // Dispose of the playing track, so it can be deleted
            // Prevents an exception being thrown where the mp3 is already in use and can't be deleted
            if (mapset.Maps.Contains(Selected.Value))
            {
                var oldTrack = AudioEngine.Track;

                AudioEngine.Track = new AudioTrackVirtual(300000);

                if (oldTrack != null && !oldTrack.IsDisposed)
                    oldTrack.Dispose();
            }

            try
            {
                var directory = Path.Combine(ConfigManager.SongDirectory.Value, mapset.Directory);

                if (!Rubbish.Move(directory) && Directory.Exists(directory))
                    ShowFallbackMapDeletionDialog("mapset", () => Directory.Delete(directory, true));
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            try
            {
                mapset.Maps.ForEach(MapDatabaseCache.RemoveMap);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            Mapsets.Remove(mapset);

            // Raise an event letting subscribers know a mapset has been deleted
            MapsetDeleted?.Invoke(typeof(MapManager), new MapsetDeletedEventArgs(mapset, index));

            // Dispose and delete the mapset's background if it exists
            lock (BackgroundHelper.MapsetBanners)
            {
                if (!BackgroundHelper.MapsetBanners.ContainsKey(mapset.Directory))
                    return;

                var banner = BackgroundHelper.MapsetBanners[mapset.Directory];

                if (banner != UserInterface.DefaultBanner)
                    banner.Dispose();

                BackgroundHelper.MapsetBanners.Remove(mapset.Directory);
            }
        }

        /// <summary>
        ///     Updates an individual map to the latest version
        /// </summary>
        /// <param name="outdated"></param>
        public static void UpdateMapToLatestVersion(Map outdated)
        {
            try
            {
                var lookup = new APIRequestMapInformation(outdated.MapId).ExecuteRequest();

                if (lookup.Status != 200)
                    throw new Exception($"Map updated failed. APIRequestMapInformation failed with status: {lookup.Status}");

                // Check if we already have the updated version of the map. If we do, just delete the old one,
                // and select the new version
                foreach (var mapset in Mapsets)
                {
                    var foundMap = mapset.Maps.Find(x => x.Md5Checksum == lookup.Map.Md5);

                    if (foundMap == null)
                        continue;

                    outdated.Mapset.Maps.Remove(outdated);

                    MapDatabaseCache.RemoveMap(outdated);

                    if (outdated.Mapset.Maps.Count == 0)
                        Mapsets.Remove(outdated.Mapset);

                    if (Selected.Value == outdated)
                        Selected.Value = foundMap;

                    PlaylistManager.UpdateMapInPlaylists(outdated, foundMap);
                    MapUpdated?.Invoke(typeof(MapManager), new MapUpdatedEventArgs(outdated, foundMap));
                    return;
                }

                // We don't have the map, so replace it and update its map information.
                var path = $"{ConfigManager.SongDirectory.Value}/{outdated.Directory}/{outdated.Path}";

                Logger.Important($"Downloading latest version of map: {outdated.Id}", LogType.Runtime);

                var client = new RestClient($"{OnlineClient.API_ENDPOINT}");
                client.DownloadData(new RestRequest($"{OnlineClient.API_ENDPOINT}/d/web/map/{outdated.MapId}", Method.GET)).SaveAs(path);

                Logger.Important($"Successfully downloaded latest version of map: {outdated.Id}", LogType.Runtime);

                var newMap = Map.FromQua(Qua.Parse(path), path);
                newMap.CalculateDifficulties();
                newMap.Id = outdated.Id;
                newMap.Mapset = outdated.Mapset;
                newMap.Directory = outdated.Directory;
                newMap.Path = outdated.Path;
                newMap.DateAdded = outdated.DateAdded;
                newMap.TimesPlayed = outdated.TimesPlayed;
                newMap.LocalOffset = outdated.LocalOffset;

                MapDatabaseCache.UpdateMap(outdated);

                outdated.Mapset.Maps.Remove(outdated);
                outdated.Mapset.Maps.Add(newMap);
                outdated.Mapset.Maps = newMap.Mapset.Maps.OrderBy(x => x.DifficultyFromMods(ModManager.Mods)).ToList();

                if (Selected.Value == outdated)
                    Selected.Value = newMap;

                PlaylistManager.UpdateMapInPlaylists(outdated, newMap);
                MapUpdated?.Invoke(typeof(MapManager), new MapUpdatedEventArgs(outdated, newMap));
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                MapUpdated?.Invoke(typeof(MapManager), new MapUpdatedEventArgs(outdated, outdated));
            }
        }

        /// <summary>
        ///     Raises an event stating that the user wants to play a song request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="map"></param>
        public static void PlaySongRequest(SongRequest request, Map map)
        {
            SongRequestPlayed?.Invoke(typeof(MapManager), new SongRequestPlayedEventArgs(request, map));
        }

        private static void ShowFallbackMapDeletionDialog(string label, Action onYes) =>
            DialogManager.Show(
                new YesNoDialog(
                    "Map Deletion",
                    $"Failed to move the {label} in the recycle bin. Would you like to delete it instead?",
                    onYes
                )
            );
    }
}
