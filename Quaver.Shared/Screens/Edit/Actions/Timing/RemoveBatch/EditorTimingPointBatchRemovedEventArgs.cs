using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch
{
    public class EditorTimingPointBatchRemovedEventArgs : EditorTimingPointBatchAddedEventArgs
    {
        public EditorTimingPointBatchRemovedEventArgs(List<TimingPointInfo> tps) : base(tps)
        {
        }
    }
}