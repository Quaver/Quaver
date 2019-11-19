using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Scheduling;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Management.Maps
{
    public class CheckboxItemPlaylistMap : ICheckboxContainerItem
    {
        /// <summary>
        /// </summary>
        private Playlist Playlist { get; }

        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="mapset"></param>
        public CheckboxItemPlaylistMap(Playlist playlist, Map mapset)
        {
            Playlist = playlist;
            Map = mapset;
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
        public bool GetSelectedState() => Playlist.Maps.Contains(Map);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void OnToggle()
        {
            // Add to playlist
            if (IsSelected)
            {
                // Only add the map if it doesn't already exist
                if (!Playlist.Maps.Contains(Map) && Playlist.Maps.All(x => x.Md5Checksum != Map.Md5Checksum))
                    Playlist.Maps.Add(Map);

                ThreadScheduler.Run(() =>
                {
                    PlaylistManager.AddMapToPlaylist(Playlist, Map);
                    PlaylistManager.EditPlaylist(Playlist, null, false);
                });
            }
            // Remove from playlist
            else
            {
                Playlist.Maps.RemoveAll(x => x == Map || x.Md5Checksum == Map.Md5Checksum);

                ThreadScheduler.Run(() =>
                {
                    PlaylistManager.RemoveMapFromPlaylist(Playlist, Map);
                    PlaylistManager.EditPlaylist(Playlist, null, false);
                });
            }

            PlaylistManager.InvokePlaylistMapsManagedEvent(Playlist);

            Logger.Important($"Changed playlist: {Playlist.Name} state to: {IsSelected} for map: {Map}",
                LogType.Runtime);
        }
    }
}