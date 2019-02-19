using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Timing
{
    public class EditorTimingPointChangeInfo
    {
        /// <summary>
        /// </summary>
        public TimingPointInfo Info { get; }

        /// <summary>
        /// </summary>
        public float OriginalTime { get; }

        /// <summary>
        /// </summary>
        public float OriginalBpm { get; }

        /// <summary>
        /// </summary>
        public float NewTime { get; }

        /// <summary>
        /// </summary>
        public float NewBpm { get; }

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="newTime"></param>
        /// <param name="newBpm"></param>
        public EditorTimingPointChangeInfo(TimingPointInfo tp, float newTime, float newBpm)
        {
            Info = tp;
            OriginalTime = Info.StartTime;
            OriginalBpm = Info.Bpm;
            NewTime = newTime;
            NewBpm = newBpm;
        }
    }
}