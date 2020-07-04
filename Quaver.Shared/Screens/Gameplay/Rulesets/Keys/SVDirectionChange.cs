namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys
{
    /// <summary>
    ///     Represents a change in the SV direction (positive to negative or negative to positive).
    /// </summary>
    public struct SVDirectionChange
    {
        /// <summary>
        ///     Start time of the SV that changed the direction.
        /// </summary>
        public float StartTime;

        /// <summary>
        ///     Position at the time of the direction change.
        /// </summary>
        public long Position;
    }
}