using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SF.AddBatch;

namespace Quaver.Shared.Screens.Edit.Actions.SF.RemoveBatch
{
    public class EditorScrollSpeedFactorBatchRemovedEventArgs : EditorScrollSpeedFactorBatchAddedEventArgs
    {
        public EditorScrollSpeedFactorBatchRemovedEventArgs(List<ScrollSpeedFactorInfo> SFs) : base(SFs)
        {
        }
    }
}