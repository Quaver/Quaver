using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Reverse
{
    public class EditorHitObjectsReversedEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public EditorHitObjectsReversedEventArgs(List<HitObjectInfo> hitObjects) => HitObjects = hitObjects;
    }
}