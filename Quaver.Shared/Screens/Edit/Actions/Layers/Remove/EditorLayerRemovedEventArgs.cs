using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Remove
{
    public class EditorLayerRemovedEventArgs : EventArgs
    {
        public EditorLayerInfo Layer { get; }

        public EditorLayerRemovedEventArgs(EditorLayerInfo l) => Layer = l;
    }
}
