using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SF.ChangeLaneMaskBatch
{
    public class EditorChangedScrollSpeedFactorLaneMaskBatchEventArgs : EventArgs
    {
        public List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        public int ActiveLaneMask { get; }
        public int InactiveLaneMask { get; }

        public EditorChangedScrollSpeedFactorLaneMaskBatchEventArgs(List<ScrollSpeedFactorInfo> SFs, int activeLaneMask,
            int inactiveLaneMask)
        {
            ScrollSpeedFactors = SFs;
            ActiveLaneMask = activeLaneMask;
            InactiveLaneMask = inactiveLaneMask;
        }
    }
}