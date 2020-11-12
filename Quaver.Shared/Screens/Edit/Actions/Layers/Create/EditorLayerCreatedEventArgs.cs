using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Create
{
    public class EditorLayerCreatedEventArgs : EventArgs
    {
        public EditorLayerInfo Layer { get; }

        public int Index { get; }

        public EditorLayerCreatedEventArgs(EditorLayerInfo l, int index)
        {
            Layer = l;
            Index = index;
        }
    }
}
