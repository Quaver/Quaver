using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SV.Add
{
    public class EditorScrollVelocityAddedEventArgs : EventArgs
    {
        public TimingGroup TimingGroup { get; }
        public SliderVelocityInfo ScrollVelocity { get; }

        public EditorScrollVelocityAddedEventArgs(SliderVelocityInfo sv, TimingGroup timingGroup)
        {
            ScrollVelocity = sv;
            TimingGroup = timingGroup;
        }
    }
}