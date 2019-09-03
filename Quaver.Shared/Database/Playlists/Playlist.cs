using System.Collections.Generic;
using Quaver.Shared.Database.Maps;
using SQLite;

namespace Quaver.Shared.Database.Playlists
{
    public class Playlist
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        ///     The name of the playlist
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The person who created the playlist
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        ///     Small description about the playlist
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     The maps that are inside of the playlist
        /// </summary>
        [Ignore]
        public List<Map> Maps { get; set; }

        /// <summary>
        ///     The game the playlist is from
        /// </summary>
        [Ignore]
        public MapGame PlaylistGame { get; set; }
    }
}