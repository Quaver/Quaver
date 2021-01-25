using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Rename
{
    public class EditorLayerRenamedEventArgs : EventArgs
    {
        public EditorLayerInfo Layer { get; }

        public string PreviousName { get; }

        public EditorLayerRenamedEventArgs(EditorLayerInfo l, string previousName)
        {
            Layer = l;
            PreviousName = previousName;
        }
    }
}
