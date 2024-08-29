using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SF.ChangeMultiplierBatch
{
    public class EditorChangedScrollSpeedFactorMultiplierBatchEventArgs : EventArgs
    {
        public List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        public float Multiplier { get; }

        public EditorChangedScrollSpeedFactorMultiplierBatchEventArgs(List<ScrollSpeedFactorInfo> SFs, float multiplier)
        {
            ScrollSpeedFactors = SFs;
            Multiplier = multiplier;
        }
    }
}