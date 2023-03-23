/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Lines
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
        ///     Timing line sprite that is associated with this object
        /// </summary>
        public TimingLine Line { get; private set; }

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

        /// <summary>
        ///     Associate the given timing line sprite with this object
        /// </summary>
        /// <param name="line"></param>
        public void Link(TimingLine line)
        {
            Line = line;
            Line.InitalizeInfo(this);
        }

        /// <summary>
        ///     Stop associating the linked timing line sprite with this object
        /// </summary>
        /// <returns></returns>
        public TimingLine Unlink()
        {
            Line.Visible = false;
            var temp = Line;
            Line = null;
            return temp;
        }
    }
}
