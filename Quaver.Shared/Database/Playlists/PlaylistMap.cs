using SQLite;

namespace Quaver.Shared.Database.Playlists
{
    public class PlaylistMap
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        ///     The id of the playlist that the map is in
        /// </summary>
        public int PlaylistId { get; set; }

        /// <summary>
        ///     The md5 hash of the map
        /// </summary>
        public string Md5 { get; set; }

        /// <summary>
        ///     The modifiers that should be applied when this map is selected in a tournament playlist.
        ///     Speed modifiers in this bitmask represent the map's playback rate.
        /// </summary>
        public long Modifiers { get; set; }
    }
}