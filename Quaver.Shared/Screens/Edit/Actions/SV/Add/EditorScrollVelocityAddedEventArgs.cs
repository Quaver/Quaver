using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SV.Add
{
    public class EditorScrollVelocityAddedEventArgs : EventArgs
    {
        public SliderVelocityInfo ScrollVelocity { get; }

        public EditorScrollVelocityAddedEventArgs(SliderVelocityInfo sv) => ScrollVelocity = sv;
    }
}