using SQLite;

namespace Quaver.Shared.Database.Maps
{
    public class OtherGameMap : Map
    {
        /// <summary>
        ///     The game that the map comes from
        /// </summary>
        public OtherGameMapDatabaseGame OriginalGame { get; set; }

        /// <summary>
        ///     Versioning system for syncing. Used to resync maps if things like parsing updates happen
        /// </summary>
        public int SyncVersion { get; set; }

        [Ignore]
        public static int OsuSyncVersion { get; set; } = 0;

        [Ignore]
        public static int EtternaSyncVersion { get; set; } = 2;
    }
}