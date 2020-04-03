using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SV.ChangeOffsetBatch
{
    public class EditorChangedScrollVelocityOffsetBatchEventArgs : EventArgs
    {
        public List<SliderVelocityInfo> ScrollVelocities { get; }

        public float Offset { get; }

        public EditorChangedScrollVelocityOffsetBatchEventArgs(List<SliderVelocityInfo> svs, float offset)
        {
            ScrollVelocities = svs;
            Offset = offset;
        }
    }
}