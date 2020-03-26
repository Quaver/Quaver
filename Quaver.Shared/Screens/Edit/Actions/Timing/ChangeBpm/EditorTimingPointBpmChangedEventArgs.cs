using System;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpm
{
    public class EditorTimingPointBpmChangedEventArgs : EventArgs
    {
        public float OriginalBpm { get; }

        public float NewBpm { get; }

        public EditorTimingPointBpmChangedEventArgs(float originalBpm, float newBpm)
        {
            OriginalBpm = originalBpm;
            NewBpm = newBpm;
        }
    }
}