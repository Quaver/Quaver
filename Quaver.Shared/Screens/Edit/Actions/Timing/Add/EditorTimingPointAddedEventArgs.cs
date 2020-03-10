using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.Add
{
    public class EditorTimingPointAddedEventArgs : EventArgs
    {
        public TimingPointInfo TimingPoint { get; }

        public EditorTimingPointAddedEventArgs(TimingPointInfo tp) => TimingPoint = tp;
    }
}