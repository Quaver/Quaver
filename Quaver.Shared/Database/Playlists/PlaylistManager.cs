using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu_database_reader.BinaryFiles;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using SQLite;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Database.Playlists
{
    public static class PlaylistManager
    {
        /// <summary>
        ///     The path of the local database
        /// </summary>
        public static readonly string DatabasePath = ConfigManager.GameDirectory + "/quaver.db";

        /// <summary>
        ///     The available playlists
        /// </summary>
        public static List<Playlist> Playlists { get; private set; } = new List<Playlist>();

        /// <summary>
        ///     The currently selected playlist
        /// </summary>
        public static Bindable<Playlist> Selected { get; } = new Bindable<Playlist>(null);

        /// <summary>
        ///     Event invoked when a new playlist has been created
        /// </summary>
        public static event EventHandler<PlaylistCreatedEventArgs> PlaylistCreated;

        /// <summary>
        ///     Event invoked when a playlist has been deleted from the game
        /// </summary>
        public static event EventHandler<PlaylistDeletedEventArgs> PlaylistDeleted;

        /// <summary>
        ///     Loads all of the maps in the database and groups them into mapsets to use
        ///     for gameplay
        /// </summary>
        public static void Load()
        {
            CreateTables();
            LoadPlaylists();

            if (ConfigManager.AutoLoadOsuBeatmaps.Value)
                LoadOsuCollections();
        }

        /// <summary>
        ///     Creates the `playlists` database table.
        /// </summary>
        private static void CreateTables()
        {
            try
            {
                var conn = new SQLiteConnection(DatabasePath);

                conn.CreateTable<Playlist>();
                Logger.Important($"Playlist Table has been created", LogType.Runtime);

                conn.CreateTable<PlaylistMap>();
                Logger.Important($"PlaylistMap table has been created", LogType.Runtime);

                conn.Close();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Loads playlists from the database
        /// </summary>
        private static void LoadPlaylists()
        {
            try
            {
                var conn = new SQLiteConnection(DatabasePath);

                var playlists = conn.Table<Playlist>().ToList();
                var playlistMaps = conn.Table<PlaylistMap>().ToList();

                // Convert playlists into a dictionary w/ the id as its key for quick access
                var playlistDictionary = playlists.ToDictionary(x => x.Id);

                // Go through each map and add it to the playlists
                foreach (var playlistMap in playlistMaps)
                {
                    // Check to see if the playlist exists
                    if (!playlistDictionary.ContainsKey(playlistMap.PlaylistId))
                        continue;

                    // Check to see if the map exists and add it
                    foreach (var mapset in MapManager.Mapsets)
                    {
                        var map = mapset.Maps.Find(x => x.Md5Checksum == playlistMap.Md5);

                        if (map == null)
                            continue;

                        playlistDictionary[playlistMap.PlaylistId].Maps.Add(map);
                    }
                }

                Playlists = Playlists.Concat(playlists).ToList();

                foreach (var playlist in playlists)
                    Logger.Important($"Loaded Quaver playlist: {playlist.Name ?? ""} w/ {playlist.Maps?.Count ?? 0} maps!", LogType.Runtime);

                conn.Close();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///    Loads osu! collections and converts them to Quaver playlists
        /// </summary>
        private static void LoadOsuCollections()
        {
            var path = ConfigManager.OsuDbPath.Value.Replace("osu!.db", "collection.db");

            if (!File.Exists(path))
                return;

            var db = CollectionDb.Read(path);

            var playlists = new List<Playlist>();

            for (var i = 0; i < db.Collections.Count; i++)
            {
                var collection = db.Collections[i];

                var playlist = new Playlist
                {
                    Id = -i,
                    Name = collection.Name,
                    Creator = "External Game",
                    Maps = new List<Map>(),
                    PlaylistGame = MapGame.Osu
                };

                foreach (var hash in collection.BeatmapHashes)
                {
                    foreach (var mapset in MapManager.Mapsets)
                    {
                        var map = mapset.Maps.Find(x => x.Md5Checksum == hash);

                        if (map == null)
                            continue;

                        playlist.Maps.Add(map);
                        break;
                    }
                }

                playlists.Add(playlist);
            }

            Playlists = Playlists.Concat(playlists).ToList();

            foreach (var playlist in playlists)
                Logger.Important($"Loaded osu! playlist: {playlist.Name ?? ""} w/ {playlist.Maps?.Count ?? 0} maps!", LogType.Runtime);
        }

        /// <summary>
        ///     Adds a playlist to the database
        /// </summary>
        /// <param name="playlist"></param>
        public static int AddPlaylist(Playlist playlist, string bannerPath = null)
        {
            var id = -1;

            try
            {
                var conn = new SQLiteConnection(DatabasePath);
                id = conn.Insert(playlist);
                conn.Close();

                Logger.Important($"Successfully added playlist: {playlist.Name} (#{playlist.Id}) (by: {playlist.Creator}) to the database",
                    LogType.Runtime);

                // Copy the banner over to the playlist directory
                if (bannerPath != null)
                {
                    var dir = $"{ConfigManager.DataDirectory}/playlists";
                    Directory.CreateDirectory(dir);
                    File.Copy(bannerPath, $"{dir}/{playlist.Id}{Path.GetExtension(bannerPath)}", true);

                    Logger.Important($"Copied over banner for playlist: {playlist.Id}", LogType.Runtime);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
            finally
            {
                if (!Playlists.Contains(playlist))
                    Playlists.Add(playlist);

                Playlists = Playlists.OrderBy(x => x.PlaylistGame).ThenBy(x => x.Name).ToList();
                Selected.Value = playlist;

                PlaylistCreated?.Invoke(typeof(PlaylistManager), new PlaylistCreatedEventArgs(playlist));
            }

            return id;
        }

        /// <summary>
        ///     Deletes a playlist from the database
        /// </summary>
        /// <param name="playlist"></param>
        public static void DeletePlaylist(Playlist playlist)
        {
            try
            {
                new SQLiteConnection(DatabasePath).Delete(playlist);
                Logger.Important($"Successfully deleted playlist: {playlist.Name} (#{playlist.Id})", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            Playlists.Remove(playlist);

            // Handle selection of the new playlist if deleted
            if (Selected.Value == playlist)
                Selected.Value = Playlists.Count != 0 ? Playlists.First() : null;

            PlaylistDeleted?.Invoke(typeof(PlaylistManager), new PlaylistDeletedEventArgs(playlist));
        }
    }
}