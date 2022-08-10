using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Colors.Add
{
    public class EditorColorSetEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public int Color { get; }

        public EditorColorSetEventArgs(List<HitObjectInfo> hitObjects, int color)
        {
            HitObjects = hitObjects;
            Color = color;
        }
    }
}