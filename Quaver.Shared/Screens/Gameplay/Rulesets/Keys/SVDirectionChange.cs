using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys
{
    /// <summary>
    ///     Represents a change in the SV direction (positive to negative or negative to positive).
    /// </summary>
    public class SVDirectionChange : IStartTime
    {
        /// <summary>
        ///     Start time of the SV that changed the direction.
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        ///     Position at the time of the direction change.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        ///     The maximum position from this direction change onwards
        /// </summary>
        public long BackPrefixMaxPosition { get; set; }

        /// <summary>
        ///     The minimum position from this direction change onwards
        /// </summary>
        public long BackPrefixMinPosition { get; set; }
    }
}