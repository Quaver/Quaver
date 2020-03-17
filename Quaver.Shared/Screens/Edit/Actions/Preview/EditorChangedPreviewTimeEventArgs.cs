using System;

namespace Quaver.Shared.Screens.Edit.Actions.Preview
{
    public class EditorChangedPreviewTimeEventArgs : EventArgs
    {
        public int PreviousTime { get; }

        public int Time { get; }

        public EditorChangedPreviewTimeEventArgs(int time, int previous)
        {
            Time = time;
            PreviousTime = previous;
        }
    }
}