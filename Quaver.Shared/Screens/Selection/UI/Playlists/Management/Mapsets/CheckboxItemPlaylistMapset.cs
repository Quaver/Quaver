using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Scheduling;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Management.Mapsets
{
    public class CheckboxItemPlaylistMapset : ICheckboxContainerItem
    {
        /// <summary>
        /// </summary>
        private Playlist Playlist { get; }

        /// <summary>
        /// </summary>
        private Mapset Mapset { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="mapset"></param>
        public CheckboxItemPlaylistMapset(Playlist playlist, Mapset mapset)
        {
            Playlist = playlist;
            Mapset = mapset;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public string GetName() => Playlist.Name;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool GetSelectedState() => Playlist.Maps.Any(x => x.Mapset == Mapset);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void OnToggle()
        {
            // Add to playlist
            if (IsSelected)
            {
                Mapset.Maps.ForEach(map =>
                {
                    // Only add the map if it doesn't already exist
                    if (!Playlist.Maps.Contains(map) && Playlist.Maps.All(x => x.Md5Checksum != map.Md5Checksum))
                        Playlist.Maps.Add(map);
                });

                ThreadScheduler.Run(() =>
                {
                    Mapset.Maps.ForEach(x => PlaylistManager.AddMapToPlaylist(Playlist, x));

                    PlaylistManager.EditPlaylist(Playlist, null,
                        ConfigManager.SelectGroupMapsetsBy.Value == GroupMapsetsBy.Playlists);
                });
            }
            // Remove from playlist
            else
            {
                Mapset.Maps.ForEach(map => Playlist.Maps.RemoveAll(x => x == map || x.Md5Checksum == map.Md5Checksum));

                ThreadScheduler.Run(() =>
                {
                    Mapset.Maps.ForEach(x => PlaylistManager.RemoveMapFromPlaylist(Playlist, x));

                    PlaylistManager.EditPlaylist(Playlist, null,
                        ConfigManager.SelectGroupMapsetsBy.Value == GroupMapsetsBy.Playlists);
                });
            }

            PlaylistManager.InvokePlaylistMapsManagedEvent(Playlist);

            Logger.Important($"Changed playlist: {Playlist.Name} state to: {IsSelected} for mapset: {Mapset.Artist} - {Mapset.Title}",
                LogType.Runtime);
        }
    }
}