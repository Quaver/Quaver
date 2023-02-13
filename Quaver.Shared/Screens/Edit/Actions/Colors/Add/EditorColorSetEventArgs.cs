using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Colors.Add
{
    public class EditorColorSetEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public List<SnapColor> OriginalHitObjectColors { get; }

        public EditorColorSetEventArgs(List<HitObjectInfo> hitObjects, List<SnapColor> colors)
        {
            HitObjects = hitObjects;
            OriginalHitObjectColors = colors;
        }
    }
}