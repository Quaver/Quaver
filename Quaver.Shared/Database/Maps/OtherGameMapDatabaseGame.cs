namespace Quaver.Shared.Database.Maps
{
    /// <summary>
    ///     We cant use <see cref="MapGame"/> here because 0 is Quaver, and we need it to be osu!.
    ///
    ///     If we want to keep compatibility between old and new databases, then we
    ///     have to create this new enum. Pre-existing (osu!) rows will already have a value of null/0.
    /// </summary>
    public enum OtherGameMapDatabaseGame
    {
        Osu,
        Etterna
    }
}