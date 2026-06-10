using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.MoveLayer;

public class EditorLayerMovedEventArgs : EventArgs
{
    public EditorLayerInfo Layer { get; }

    public int OriginalIndex { get; }

    public int TargetIndex { get; }

    public EditorLayerMovedEventArgs(EditorLayerInfo layer, int originalIndex, int targetIndex)
    {
        Layer = layer;
        OriginalIndex = originalIndex;
        TargetIndex = targetIndex;
    }
}