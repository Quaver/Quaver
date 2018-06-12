namespace Quaver.States.Gameplay.Replays
{
    public class ReplayFrame
    {
        /// <summary>
        ///     The time in the replay since the last frame.
        /// </summary>
        public float Time { get; }

        /// <summary>
        ///     The keys that were pressed during this frame.
        /// </summary>
        public ReplayKeyPressState Keys { get; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="time"></param>
        /// <param name="keys"></param>
        public ReplayFrame(float time, ReplayKeyPressState keys)
        {
            Time = time;
            Keys = keys;
        }
    }
}