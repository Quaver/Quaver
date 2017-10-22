namespace Quaver.Gameplay
{
    internal class TimingObject
    {
        /// <summary>
        ///     The time (in ms) which the timing point starts at
        /// </summary>
        public float TargetTime { get; set; }

        /// <summary>
        ///     The BPM of this timing object.
        /// </summary>
        public float BPM { get; set; }

        /// <summary>
        ///     The SV multiplier (scroll velocity multiplier) of this object.
        /// </summary>
        public float SvMultiplier { get; set; }
    }
}