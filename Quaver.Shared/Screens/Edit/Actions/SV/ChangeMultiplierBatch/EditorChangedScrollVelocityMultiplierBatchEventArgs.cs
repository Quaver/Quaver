using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SV.ChangeMultiplierBatch
{
    public class EditorChangedScrollVelocityMultiplierBatchEventArgs : EventArgs
    {
        public List<SliderVelocityInfo> ScrollVelocities { get; }

        public float Multiplier { get; }

        public EditorChangedScrollVelocityMultiplierBatchEventArgs(List<SliderVelocityInfo> svs, float multiplier)
        {
            ScrollVelocities = svs;
            Multiplier = multiplier;
        }
    }
}