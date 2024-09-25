using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;

namespace Quaver.Shared.Screens.Edit.Actions.SV.Remove
{
    public class EditorScrollVelocityRemovedEventArgs : EditorScrollVelocityAddedEventArgs
    {
        public EditorScrollVelocityRemovedEventArgs(SliderVelocityInfo sv, TimingGroup timingGroup) : base(sv, timingGroup)
        {
        }
    }
}