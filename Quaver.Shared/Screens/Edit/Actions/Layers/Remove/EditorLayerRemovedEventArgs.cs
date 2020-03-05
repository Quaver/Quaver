using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Remove
{
    public class EditorLayerRemovedEventArgs : EditorLayerCreatedEventArgs
    {
        public EditorLayerRemovedEventArgs(EditorLayerInfo l) : base(l)
        {
        }
    }
}