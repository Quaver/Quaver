namespace Quaver.States.Gameplay.Replays
{
    public class ReplayFrame
    {
        /// <summary>
        ///     The time in the replay since the last frame.
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        ///     The keys that were pressed during this frame.
        /// </summary>
        public ReplayKeyPressState Keys { get; set; }

        public ReplayFrame(int time, ReplayKeyPressState keys)
        {
            Time = time;
            Keys = keys;
        }
    }
}