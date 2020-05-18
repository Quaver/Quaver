using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.Add;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.Remove
{
    public class EditorTimingPointRemovedEventArgs : EditorTimingPointAddedEventArgs
    {
        public EditorTimingPointRemovedEventArgs(TimingPointInfo tp) : base(tp)
        {
        }
    }
}