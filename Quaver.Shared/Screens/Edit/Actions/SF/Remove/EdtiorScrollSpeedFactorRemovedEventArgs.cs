using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SF.Add;

namespace Quaver.Shared.Screens.Edit.Actions.SF.Remove
{
    public class EditorScrollSpeedFactorRemovedEventArgs : EditorScrollSpeedFactorAddedEventArgs
    {
        public EditorScrollSpeedFactorRemovedEventArgs(ScrollSpeedFactorInfo SF) : base(SF)
        {
        }
    }
}