using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpmBatch
{
    public class EditorChangedTimingPointBpmBatchEventArgs : EventArgs
    {
        public List<TimingPointInfo> TimingPoints { get; }

        public EditorChangedTimingPointBpmBatchEventArgs(List<TimingPointInfo> tps) => TimingPoints = tps;
    }
}