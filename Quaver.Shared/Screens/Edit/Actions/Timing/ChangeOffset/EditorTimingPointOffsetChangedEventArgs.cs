using System;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset
{
    public class EditorTimingPointOffsetChangedEventArgs : EventArgs
    {
        public float OriginalOffset { get; }

        public float NewOffset { get; }

        public EditorTimingPointOffsetChangedEventArgs(float originalOffset, float newOffset)
        {
            OriginalOffset = originalOffset;
            NewOffset = newOffset;
        }
    }
}