using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SF.AddBatch
{
    public class EditorScrollSpeedFactorBatchAddedEventArgs : EventArgs
    {
        public List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        public EditorScrollSpeedFactorBatchAddedEventArgs(List<ScrollSpeedFactorInfo> SFs) => ScrollSpeedFactors = SFs;
    }
}