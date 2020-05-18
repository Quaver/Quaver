using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffsetBatch
{
    public class EditorChangedTimingPointOffsetBatchEventArgs : EventArgs
    {
        public List<TimingPointInfo> TimingPoints { get; }

        public float Offset { get; }

        public EditorChangedTimingPointOffsetBatchEventArgs(List<TimingPointInfo> tps, float offset)
        {
            TimingPoints = tps;
            Offset = offset;
        }
    }
}