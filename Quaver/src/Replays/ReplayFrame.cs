using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Replays
{
    internal class ReplayFrame
    {
        /// <summary>
        ///     Holds the game time the frame was captured at
        /// </summary>
        internal long GameTime { get; set; }

        /// <summary>
        ///     The time in the song this replay occurred at.
        /// </summary>
        internal double SongTime { get; set; }

        /// <summary>
        ///     The difference in song time of the current and last frames taken in milliseconds
        /// </summary>
        internal double TimeSinceLastFrame { get; set; }

        /// <summary>
        ///     Keeps track of if this frame is a skip frame.
        /// </summary>
        internal bool IsSkipFrame { get; set; }

        /// <summary>
        ///     The state of the keys during this frame.
        /// </summary>
        internal KeyPressState KeyPressState { get; set; }
    }
}
