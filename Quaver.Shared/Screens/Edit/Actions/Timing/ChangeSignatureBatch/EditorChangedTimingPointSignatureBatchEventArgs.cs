using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignatureBatch
{
    public class EditorChangedTimingPointSignatureBatchEventArgs : EventArgs
    {
        public List<TimingPointInfo> TimingPoints { get; }

        public EditorChangedTimingPointSignatureBatchEventArgs(List<TimingPointInfo> tps) => TimingPoints = tps;
    }
}