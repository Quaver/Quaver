using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SF.ChangeOffsetBatch
{
    public class EditorChangedScrollSpeedFactorOffsetBatchEventArgs : EventArgs
    {
        public List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        public float Offset { get; }

        public EditorChangedScrollSpeedFactorOffsetBatchEventArgs(List<ScrollSpeedFactorInfo> SFs, float offset)
        {
            ScrollSpeedFactors = SFs;
            Offset = offset;
        }
    }
}