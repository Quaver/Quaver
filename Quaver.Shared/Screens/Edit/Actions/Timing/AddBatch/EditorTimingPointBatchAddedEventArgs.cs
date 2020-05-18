using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch
{
    public class EditorTimingPointBatchAddedEventArgs : EventArgs
    {
        public List<TimingPointInfo> TimingPoints { get; }

        public EditorTimingPointBatchAddedEventArgs(List<TimingPointInfo> tps) => TimingPoints = tps;
    }
}