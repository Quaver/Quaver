using System;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignature
{
    public class EditorTimingPointSignatureChangedEventArgs : EventArgs
    {
        public int OriginalSignature { get; }

        public int NewSignature { get; }

        public EditorTimingPointSignatureChangedEventArgs(int originalSignature, int newSignature)
        {
            OriginalSignature = originalSignature;
            NewSignature = newSignature;
        }
    }
}