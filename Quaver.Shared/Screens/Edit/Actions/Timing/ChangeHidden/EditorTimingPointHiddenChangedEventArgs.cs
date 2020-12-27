using System;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeHidden
{
    public class EditorTimingPointHiddenChangedEventArgs : EventArgs
    {
        public bool OriginalHidden { get; }

        public bool NewHidden { get; }

        public EditorTimingPointHiddenChangedEventArgs(bool originalHidden, bool newHidden)
        {
            OriginalHidden = originalHidden;
            NewHidden = newHidden;
        }
    }
}