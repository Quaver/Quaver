using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Colors
{
    public class EditorLayerColorChangedEventArgs : EventArgs
    {
        public EditorLayerInfo Layer { get; }

        public Color OldColor { get; }

        public Color NewColor { get; }

        public EditorLayerColorChangedEventArgs(EditorLayerInfo layer, Color oldColor, Color newColor)
        {
            Layer = layer;
            OldColor = oldColor;
            NewColor = newColor;
        }
    }
}