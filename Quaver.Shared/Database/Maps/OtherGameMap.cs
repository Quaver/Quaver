namespace Quaver.Shared.Database.Maps
{
    public class OtherGameMap : Map
    {
        /// <summary>
        ///     The game that the map comes from
        /// </summary>
        public OtherGameMapDatabaseGame OriginalGame { get; set; }
    }
}