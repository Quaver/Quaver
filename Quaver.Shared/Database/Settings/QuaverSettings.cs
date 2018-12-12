using SQLite;

namespace Quaver.Shared.Database.Settings
{
    /// <summary>
    ///     A row table that consists of the versions of the difficulty/rating/score calculators
    ///     that we have, so we can update the map database cache with new values whenever
    ///     we upgrade them.
    /// </summary>
    public class QuaverSettings
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public string VersionDifficultyProcessorKeys { get; set; }

        public string VersionRatingProcessorKeys { get; set; }

        public string VersionScoreProcessorKeys { get; set; }
    }
}