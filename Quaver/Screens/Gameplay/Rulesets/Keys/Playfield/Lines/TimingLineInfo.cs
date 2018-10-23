namespace Quaver.Screens.Gameplay.Rulesets.Keys.Playfield.Lines
{
    public class TimingLineInfo
    {
        /// <summary>
        ///     Time at which the timing line reaches the receptor
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        ///     The Timing line's Y offset from the receptor
        /// </summary>
        public long TrackOffset { get; set; }

        /// <summary>
        ///     information used for the lines representing every 4 beats on the playfield
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="offset"></param>
        public TimingLineInfo (float startTime, long offset)
        {
            StartTime = startTime;
            TrackOffset = offset;
        }
    }
}
