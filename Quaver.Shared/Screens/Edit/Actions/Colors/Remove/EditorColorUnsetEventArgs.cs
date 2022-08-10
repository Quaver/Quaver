using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Colors.Add;

namespace Quaver.Shared.Screens.Edit.Actions.Colors.Remove
{
    public class EditorColorUnsetEventArgs : EditorColorSetEventArgs
    {
        public EditorColorUnsetEventArgs(List<HitObjectInfo> hitObjects, int color) : base(hitObjects, color)
        {
        }
    }
}