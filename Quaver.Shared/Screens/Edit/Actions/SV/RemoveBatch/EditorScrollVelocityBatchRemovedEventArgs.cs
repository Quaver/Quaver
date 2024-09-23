using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.AddBatch;

namespace Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch
{
    public class EditorScrollVelocityBatchRemovedEventArgs : EditorScrollVelocityBatchAddedEventArgs
    {
        public EditorScrollVelocityBatchRemovedEventArgs(List<SliderVelocityInfo> svs, ScrollGroup scrollGroup) : base(svs, scrollGroup)
        {
        }
    }
}