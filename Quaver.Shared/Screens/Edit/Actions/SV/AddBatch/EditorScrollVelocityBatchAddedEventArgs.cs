using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SV.AddBatch
{
    public class EditorScrollVelocityBatchAddedEventArgs : EventArgs
    {
        public List<SliderVelocityInfo> ScrollVelocities { get; }

        public EditorScrollVelocityBatchAddedEventArgs(List<SliderVelocityInfo> svs) => ScrollVelocities = svs;
    }
}